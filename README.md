# Conforto da Tela

Filtro de tela para Windows que reduz o brilho além do limite disponível nas configurações do sistema. Foi criado para ajudar quem precisa trabalhar ou estudar com mais conforto em notebooks simples, mesmo quando o brilho do Windows já está em 0%.

## Download

Baixe a versão pronta em **[Releases](../../releases/latest)** e execute `Conforto da Tela.exe`. O aplicativo é portátil: não precisa de instalação nem de arquivos extras ao lado do EXE.

## Como usar

1. Abra o `Conforto da Tela.exe`.
2. Escolha a intensidade desejada no controle deslizante.
3. Use **Pausar filtro** quando quiser voltar à imagem normal.
4. Ative **Iniciar com o Windows** se quiser que o filtro seja aplicado automaticamente.

O aplicativo salva as preferências de cada usuário em sua própria pasta de dados do Windows. A tecla `Ctrl + Alt + F10` alterna entre filtro ativo e pausado.

## Identificação do aplicativo

Fabricante exibido nas propriedades do arquivo: **Getulio D-Eng de Soft**.

O Windows pode exibir um aviso do SmartScreen em versões novas ou pouco baixadas. Esse aviso é baseado em reputação e assinatura digital; o nome do fabricante já está incorporado no executável.

## Compatibilidade

Windows 10 e Windows 11 com .NET Framework 4.x disponível. O filtro cobre os monitores reconhecidos pelo Windows e acompanha a abertura de novas janelas comuns.

## Código-fonte

O código está em [`src/ConfortoDaTela.cs`](src/ConfortoDaTela.cs). A versão distribuída é compilada como um único executável para facilitar o uso por qualquer pessoa.

## Compilar

Com o compilador do .NET Framework instalado:

```powershell
csc /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Xml.Linq.dll src/ConfortoDaTela.cs
```

## Política de assinatura de código

Quando aprovado, o projeto usará **Free code signing provided by SignPath.io, certificate by SignPath Foundation**.

- Committers e revisores: Getulio D-Eng de Soft.
- Aprovador das versões: Getulio D-Eng de Soft.
- O aplicativo não transfere informações para outros sistemas, exceto quando o usuário solicita uma ação de rede.

## Licença

Este projeto está disponível sob a licença MIT. Consulte [`LICENSE`](LICENSE).

