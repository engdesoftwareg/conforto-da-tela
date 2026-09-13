using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;

using System.Reflection;

[assembly: AssemblyTitle("Conforto da Tela")]
[assembly: AssemblyDescription("Filtro de tela para reduzir o brilho além do limite do Windows")]
[assembly: AssemblyCompany("Getulio D-Eng de Soft")]
[assembly: AssemblyProduct("Conforto da Tela")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Getulio D-Eng de Soft")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

namespace TelaConforto {
static class Native {
 internal delegate void WindowEvent(IntPtr hook,uint evt,IntPtr hwnd,int objectId,int childId,uint thread,uint time);
 [DllImport("user32.dll")] internal static extern IntPtr SetWinEventHook(uint first,uint last,IntPtr module,WindowEvent callback,uint process,uint thread,uint flags);
 [DllImport("user32.dll")] internal static extern bool UnhookWinEvent(IntPtr hook);
 [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hwnd,IntPtr after,int x,int y,int w,int h,uint flags);
 [DllImport("user32.dll")] internal static extern IntPtr GetWindow(IntPtr hwnd,uint command);
 [DllImport("user32.dll")] internal static extern bool IsWindowVisible(IntPtr hwnd);
 [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint process);
 [DllImport("user32.dll")] internal static extern bool GetWindowRect(IntPtr hwnd,out WindowRect rect);
 [StructLayout(LayoutKind.Sequential)] internal struct WindowRect {internal int Left,Top,Right,Bottom;}

 [DllImport("user32.dll")] internal static extern bool SetProcessDPIAware();
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] internal static extern IntPtr FindWindow(string cls,string title);
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] internal static extern IntPtr SendMessage(IntPtr hwnd,int msg,IntPtr wp,ref CopyData data);
 [DllImport("user32.dll")] internal static extern bool RegisterHotKey(IntPtr hwnd,int id,uint mods,uint key);
 [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr hwnd,int id);
 [DllImport("user32.dll")] internal static extern bool GetLayeredWindowAttributes(IntPtr hwnd,out uint color,out byte alpha,out uint flags);
 [DllImport("user32.dll")] internal static extern bool PostMessage(IntPtr hwnd,int msg,IntPtr wp,IntPtr lp);
 [StructLayout(LayoutKind.Sequential)] internal struct CopyData { public IntPtr id; public int length; public IntPtr data; }
 internal static void Send(IntPtr hwnd,string command) {
  var data=new CopyData {id=new IntPtr(1),length=(command.Length+1)*2,data=Marshal.StringToHGlobalUni(command)};
  try {SendMessage(hwnd,0x004A,IntPtr.Zero,ref data);} finally {Marshal.FreeHGlobal(data.data);}
 }
}
static class Files {
 internal static string Data=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ConfortoDaTela");
 internal static string Settings=Path.Combine(Data,"preferencias.xml");
 internal static string Status=Path.Combine(Data,"estado.txt");
 internal static string Startup=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),"Filtro de conforto da tela.lnk");
 internal static object Invoke(object obj,string method,params object[] args) {return obj.GetType().InvokeMember(method,System.Reflection.BindingFlags.InvokeMethod,null,obj,args);}
 internal static object Get(object obj,string name) {return obj.GetType().InvokeMember(name,System.Reflection.BindingFlags.GetProperty,null,obj,null);}
 internal static void Set(object obj,string name,object value) {obj.GetType().InvokeMember(name,System.Reflection.BindingFlags.SetProperty,null,obj,new object[]{value});}
 internal static bool AutoStart() {
  if(!File.Exists(Startup)) return false;
  object shell=null,link=null;
  try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));link=Invoke(shell,"CreateShortcut",Startup);
   return String.Equals((string)Get(link,"TargetPath"),Application.ExecutablePath,StringComparison.OrdinalIgnoreCase);
  } catch{return false;} finally {if(link!=null)Marshal.ReleaseComObject(link);if(shell!=null)Marshal.ReleaseComObject(shell);}
 }
 internal static void SetAutoStart(bool enabled) {
  if(!enabled) {if(AutoStart())File.Delete(Startup);return;}
  object shell=null,link=null;
  try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));link=Invoke(shell,"CreateShortcut",Startup);
   Set(link,"TargetPath",Application.ExecutablePath);Set(link,"Arguments","--background");Set(link,"WorkingDirectory",Path.GetDirectoryName(Application.ExecutablePath));
   Set(link,"Description","Conforto da Tela — iniciar com a intensidade salva");Set(link,"IconLocation",Application.ExecutablePath+",0");
   Invoke(link,"Save");
  } finally {if(link!=null)Marshal.ReleaseComObject(link);if(shell!=null)Marshal.ReleaseComObject(shell);}
 }
 internal static int LoadPercent() {try {return Math.Max(5,Math.Min(70,(int)XDocument.Load(Settings).Root.Element("porcentagem")));}catch{return 30;}}
 internal static void Save(int p) {Directory.CreateDirectory(Data);new XDocument(new XElement("conforto",new XElement("porcentagem",p))).Save(Settings);}
}
class Shade:Form {
 protected override bool ShowWithoutActivation {get{return true;}}
 protected override CreateParams CreateParams {get{var cp=base.CreateParams;cp.ExStyle|=0x80000|0x20|0x80|0x08000000;return cp;}}
 internal Shade(Rectangle bounds,int percent) {FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.Manual;Bounds=bounds;BackColor=Color.Black;Opacity=percent/100.0;ShowInTaskbar=false;TopMost=true;Text="Conforto da Tela — filtro";}
}
class LevelSlider:Control {
 internal int Value=30;
 internal event EventHandler Changed;
 internal LevelSlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);TabStop=true;Cursor=Cursors.Hand;AccessibleName="Porcentagem de escurecimento";AccessibleRole=AccessibleRole.Slider;}
 internal void Change(int value){Value=Math.Max(5,Math.Min(70,value));Invalidate();if(Changed!=null)Changed(this,EventArgs.Empty);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;int x=16+(int)((Width-32)*(Value-5)/65.0);int y=Height/2;using(var p=new Pen(Color.FromArgb(66,79,93),6)){g.DrawLine(p,16,y,Width-16,y);}using(var p=new Pen(Color.FromArgb(93,222,187),6)){g.DrawLine(p,16,y,x,y);}using(var b=new SolidBrush(Color.FromArgb(93,222,187))){g.FillEllipse(b,x-10,y-10,20,20);}if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(2,2,Width-4,Height-4));}
 void UpdateMouse(int x){Change(5+(int)Math.Round((x-16)*65.0/(Width-32)));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Focus();Capture=true;UpdateMouse(e.X);}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture)UpdateMouse(e.X);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);Capture=false;}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Home||key==Keys.End||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Left)Change(Value-5);if(e.KeyCode==Keys.Right)Change(Value+5);if(e.KeyCode==Keys.Home)Change(5);if(e.KeyCode==Keys.End)Change(70);}
}
class MainPanel:Form {
 internal const string WindowTitle="Conforto da Tela";
 readonly List<Shade> shades=new List<Shade>();
 readonly Color bg=Color.FromArgb(18,26,36),card=Color.FromArgb(28,39,52),muted=Color.FromArgb(175,190,205),accent=Color.FromArgb(93,222,187);
 NotifyIcon tray;Label percentage,state,saveNote;Button toggle;LevelSlider slider;CheckBox startup;
 int percent;bool enabled=true,exiting,initializing=true,hotkey;System.Windows.Forms.Timer saveTimer;

 System.Windows.Forms.Timer layerTimer;
 Native.WindowEvent layerCallback;
 IntPtr foregroundHook,showHook;
 bool layerQueued,layerStopped;
 readonly uint ownProcess=(uint)Process.GetCurrentProcess().Id;
 internal MainPanel(bool background) {
  Native.SetProcessDPIAware();Text=WindowTitle;BackColor=bg;ForeColor=Color.White;Font=new Font("Segoe UI",10);
  AutoScaleMode=AutoScaleMode.Dpi;ClientSize=new Size(580,660);FormBorderStyle=FormBorderStyle.FixedSingle;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;
  Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);percent=Files.LoadPercent();
  LabelAt(this,"CONFORTO DA TELA",28,22,520,24,10,accent,FontStyle.Bold);
  LabelAt(this,"Sua tela, no seu ritmo.",28,57,524,44,25,Color.White,FontStyle.Bold);
  LabelAt(this,"Ajuste a imagem mesmo com o brilho do Windows no mínimo.",30,110,520,38,10,muted,FontStyle.Regular);
  Panel control=new Panel{Location=new Point(28,159),Size=new Size(524,282),BackColor=card};Controls.Add(control);
  LabelAt(control,"ESCURECIMENTO",22,20,265,24,10,muted,FontStyle.Bold);
  state=LabelAt(control,"",302,20,202,24,10,accent,FontStyle.Bold);state.TextAlign=ContentAlignment.MiddleRight;
  percentage=LabelAt(control,"",20,50,300,69,40,Color.White,FontStyle.Bold);
  ButtonAt(control,"− 5",354,68,68,40,delegate{slider.Change(percent-5);},false);
  ButtonAt(control,"+ 5",432,68,68,40,delegate{slider.Change(percent+5);},false);
  slider=new LevelSlider{Location=new Point(17,130),Size=new Size(489,42),BackColor=card,Value=percent};
  slider.Changed+=delegate{percent=slider.Value;Apply();saveNote.Text="Salvando ajuste…";saveTimer.Stop();saveTimer.Start();};control.Controls.Add(slider);
  LabelAt(control,"Mais claro",23,175,180,20,9,muted,FontStyle.Regular);
  var dark=LabelAt(control,"Mais escuro",330,175,169,20,9,muted,FontStyle.Regular);dark.TextAlign=ContentAlignment.TopRight;
  ButtonAt(control,"Suave · 15%",22,220,151,38,delegate{slider.Change(15);},false);
  ButtonAt(control,"Médio · 30%",186,220,151,38,delegate{slider.Change(30);},false);
  ButtonAt(control,"Intenso · 50%",350,220,151,38,delegate{slider.Change(50);},false);
  toggle=ButtonAt(this,"",28,459,524,46,delegate{enabled=!enabled;Apply();},true);
  saveNote=LabelAt(this,"A intensidade é salva automaticamente.",30,516,520,24,9,muted,FontStyle.Regular);
  startup=new CheckBox{Text="Ligar automaticamente ao entrar no Windows",Location=new Point(30,554),Size=new Size(522,30),ForeColor=Color.White,Checked=Files.AutoStart(),AccessibleName="Iniciar com o Windows"};
  startup.CheckedChanged+=delegate{if(initializing)return;try{Files.SetAutoStart(startup.Checked);WriteStatus();}catch(Exception ex){MessageBox.Show(this,"Não foi possível atualizar a inicialização.\n"+ex.Message,WindowTitle,MessageBoxButtons.OK,MessageBoxIcon.Information);initializing=true;startup.Checked=Files.AutoStart();initializing=false;}};Controls.Add(startup);
  LabelAt(this,"Ctrl + Alt + F10 pausa o filtro. Fechar esta janela mantém o filtro ativo.",30,606,522,38,9,muted,FontStyle.Regular);
  saveTimer=new System.Windows.Forms.Timer{Interval=400};saveTimer.Tick+=delegate{saveTimer.Stop();SaveSettings();};
  var menu=new ContextMenuStrip();menu.Items.Add("Abrir painel",null,delegate{ShowPanel();});menu.Items.Add("Ligar / pausar filtro",null,delegate{enabled=!enabled;Apply();});
  menu.Items.Add(new ToolStripSeparator());menu.Items.Add("Sair e desligar filtro",null,delegate{ExitApp();});
  tray=new NotifyIcon{Icon=Icon,Visible=true,ContextMenuStrip=menu};tray.DoubleClick+=delegate{ShowPanel();};
  var hwnd=Handle;hotkey=Native.RegisterHotKey(hwnd,1,0x4003,0x79);
  Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplayChanged;
  Microsoft.Win32.SystemEvents.SessionEnding+=SessionEnding;
  RebuildShades();initializing=false;
  StartLayerGuard();
  if(!background)ShowPanel();
 }
 Label LabelAt(Control parent,string text,int x,int y,int w,int h,float size,Color color,FontStyle style){var label=new Label{Text=text,Location=new Point(x,y),Size=new Size(w,h),Font=new Font("Segoe UI",size,style),ForeColor=color,BackColor=Color.Transparent};parent.Controls.Add(label);return label;}
 Button ButtonAt(Control parent,string text,int x,int y,int w,int h,EventHandler click,bool primary){var b=new Button{Text=text,Location=new Point(x,y),Size=new Size(w,h),FlatStyle=FlatStyle.Flat,BackColor=primary?accent:Color.FromArgb(37,51,66),ForeColor=primary?bg:Color.White,Font=new Font("Segoe UI",10,FontStyle.Bold),Cursor=Cursors.Hand,UseVisualStyleBackColor=false};b.FlatAppearance.BorderSize=0;b.Click+=click;parent.Controls.Add(b);return b;}
 void SessionEnding(object sender,Microsoft.Win32.SessionEndingEventArgs e){SaveSettings();}
 void DisplayChanged(object sender,EventArgs e){if(!IsDisposed&&IsHandleCreated)BeginInvoke((MethodInvoker)delegate{RebuildShades();});}
 void RebuildShades(){foreach(var s in shades){s.Close();s.Dispose();}shades.Clear();foreach(var screen in Screen.AllScreens){var s=new Shade(screen.Bounds,percent);shades.Add(s);}Apply();}
 void Apply(){
  foreach(var s in shades){s.Opacity=percent/100.0;if(enabled){if(!s.Visible)s.Show();}else s.Hide();}
  percentage.Text=percent+"%";state.Text=enabled?"●  ATIVO":"○  PAUSADO";state.ForeColor=enabled?accent:muted;
  toggle.Text=enabled?"Pausar filtro":"Ligar filtro";tray.Text=enabled?"Conforto da Tela · "+percent+"%":"Conforto da Tela · pausado";
  KeepLayersAbove();WriteStatus();
 }
 void SaveSettings(){try{Files.Save(percent);saveNote.Text="Ajuste salvo · "+percent+"%";}catch{saveNote.Text="Não foi possível salvar. Verifique a pasta de preferências.";}}
 internal void ShowPanel(){Show();WindowState=FormWindowState.Normal;TopMost=true;BringToFront();Activate();WriteStatus();}
 internal void Command(string command){
  if(command=="--off"){enabled=false;Apply();}
  else if(command=="--on"||command=="--background"){enabled=true;Apply();}
  else if(command=="--exit"){ExitApp();}
  else if(command.StartsWith("--set=")){int value;if(Int32.TryParse(command.Substring(6),out value)){slider.Change(value);SaveSettings();}}
  else if(command=="--status"){WriteStatus();}
  else if(command=="--preview"){using(var bitmap=new Bitmap(ClientSize.Width,ClientSize.Height)){DrawToBitmap(bitmap,new Rectangle(Point.Empty,ClientSize));bitmap.Save(Path.Combine(Files.Data,"painel.png"),System.Drawing.Imaging.ImageFormat.Png);}WriteStatus();}
  else if(command=="--hide"){Hide();WriteStatus();}
  else if(command=="--refresh"){initializing=true;startup.Checked=Files.AutoStart();initializing=false;WriteStatus();}
  else ShowPanel();
 }
 internal void WriteStatus(){
  try{Directory.CreateDirectory(Files.Data);var text=new StringBuilder();
   text.AppendLine("Porcentagem="+percent);text.AppendLine("Ativo="+enabled);text.AppendLine("PainelVisivel="+Visible);text.AppendLine("InicioAutomatico="+Files.AutoStart());text.AppendLine("AtalhoRegistrado="+hotkey);text.AppendLine("Processo="+Process.GetCurrentProcess().Id);
   foreach(var s in shades){uint color,flags;byte alpha;bool read=Native.GetLayeredWindowAttributes(s.Handle,out color,out alpha,out flags);text.AppendLine("Filtro="+s.Handle.ToInt64()+";Visivel="+s.Visible+";Alpha="+alpha+";Leitura="+read+";Tela="+s.Bounds);}
   File.WriteAllText(Files.Status,text.ToString(),Encoding.UTF8);
  }catch{}
 }

 void StartLayerGuard() {
  layerCallback=OnWindowEvent;
  foregroundHook=Native.SetWinEventHook(3,3,IntPtr.Zero,layerCallback,0,0,2);
  showHook=Native.SetWinEventHook(0x8002,0x8002,IntPtr.Zero,layerCallback,0,0,2);
  layerTimer=new System.Windows.Forms.Timer{Interval=250};
  layerTimer.Tick+=delegate{KeepLayersAbove();};
  layerTimer.Start();
 }
 void OnWindowEvent(IntPtr hook,uint evt,IntPtr hwnd,int objectId,int childId,uint thread,uint time) {
  if(layerStopped||exiting||!enabled||hwnd==IntPtr.Zero||IsDisposed||!IsHandleCreated) return;
  if(evt==0x8002 && (objectId!=0 || childId!=0)) return;
  if(layerQueued) return;
  layerQueued=true;
  try {BeginInvoke((MethodInvoker)delegate{layerQueued=false;KeepLayersAbove();});}
  catch(InvalidOperationException){layerQueued=false;}
 }
 bool IsCovered(Shade shade) {
  IntPtr current=Native.GetWindow(shade.Handle,3);
  for(int count=0;current!=IntPtr.Zero && count<512;count++) {
   if(Native.IsWindowVisible(current)) {
    uint pid;Native.GetWindowThreadProcessId(current,out pid);
    Native.WindowRect rect;
    if(pid!=ownProcess && Native.GetWindowRect(current,out rect) &&
       rect.Right>shade.Left && rect.Left<shade.Right && rect.Bottom>shade.Top && rect.Top<shade.Bottom) return true;
   }
   IntPtr next=Native.GetWindow(current,3);
   if(next==current) break;
   current=next;
  }
  return false;
 }
 void KeepLayersAbove() {
  if(layerStopped||exiting||!enabled||IsDisposed) return;
  bool raised=false;
  foreach(var shade in shades) {
   if(shade.IsDisposed||!shade.Visible) continue;
   if(IsCovered(shade)) {
    raised=Native.SetWindowPos(shade.Handle,new IntPtr(-1),0,0,0,0,0x0213)||raised;
   }
  }
  // Keep our visible controls usable without activating them or stealing focus.
  if(raised && Visible && WindowState!=FormWindowState.Minimized)
   Native.SetWindowPos(Handle,new IntPtr(-1),0,0,0,0,0x0213);
 }
 void StopLayerGuard() {
  layerStopped=true;
  if(layerTimer!=null){layerTimer.Stop();layerTimer.Dispose();}
  if(foregroundHook!=IntPtr.Zero){Native.UnhookWinEvent(foregroundHook);foregroundHook=IntPtr.Zero;}
  if(showHook!=IntPtr.Zero){Native.UnhookWinEvent(showHook);showHook=IntPtr.Zero;}
 }

 void ExitApp(){exiting=true;SaveSettings();Close();Application.Exit();}
 protected override void OnFormClosing(FormClosingEventArgs e){if(!exiting&&e.CloseReason==CloseReason.UserClosing){e.Cancel=true;Hide();WriteStatus();return;}base.OnFormClosing(e);}
 protected override void OnFormClosed(FormClosedEventArgs e){StopLayerGuard();SaveSettings();Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplayChanged;Microsoft.Win32.SystemEvents.SessionEnding-=SessionEnding;if(hotkey)Native.UnregisterHotKey(Handle,1);saveTimer.Dispose();tray.Visible=false;tray.Dispose();foreach(var s in shades){s.Close();s.Dispose();}base.OnFormClosed(e);}
 protected override void WndProc(ref Message m){if(m.Msg==0x004A){var data=(Native.CopyData)Marshal.PtrToStructure(m.LParam,typeof(Native.CopyData));Command(Marshal.PtrToStringUni(data.data));m.Result=new IntPtr(1);return;}if(m.Msg==0x0312){enabled=false;Apply();return;}base.WndProc(ref m);}
}
static class Program {
 [STAThread] static void Main(string[] args){
  string command=args.Length>0?args[0]:"--show";bool first;using(var mutex=new Mutex(true,@"Local\ConfortoDaTela.DesktopKit.v2",out first)){
   if(!first){var hwnd=Native.FindWindow(null,MainPanel.WindowTitle);if(hwnd!=IntPtr.Zero)Native.Send(hwnd,command);return;}
   if(command=="--off"||command=="--exit"||command=="--status")return;
   Native.SetProcessDPIAware();Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   try{var main=new MainPanel(command=="--background");Application.Run();}
   catch(Exception ex){Directory.CreateDirectory(Files.Data);File.WriteAllText(Path.Combine(Files.Data,"erro.txt"),ex.ToString());MessageBox.Show("Não foi possível abrir o painel. Os detalhes foram salvos em:\n"+Files.Data,MainPanel.WindowTitle,MessageBoxButtons.OK,MessageBoxIcon.Information);}
  }
 }
}
}
