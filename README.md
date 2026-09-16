# Visualizar Markdown

Visualizar Markdown 1.0 e um aplicativo desktop para Windows que abre arquivos `.md` em uma visualizacao inspirada no preview do VS Code, com layout de pagina estilo Word, modo codigo editavel, realce de sintaxe e exportacao para PDF pela impressora do Windows.

## Recursos

- Abrir arquivos `.md`, `.markdown` e `.mdown`.
- Preview renderizado com margem de pagina, tipografia limpa e temas claro/escuro.
- Modo codigo editavel, com botao para salvar o arquivo Markdown.
- Realce de blocos de codigo para `html`, `xml`, `sql`, `php`, `javascript`, `json`, `css`, `csharp`, `powershell`, `env`, `.env`, `dotenv`, `environment` e `ini`.
- Exportacao para PDF usando a janela de impressao do Windows, escolhendo `Microsoft Print to PDF`.
- Icone proprio no executavel e na janela do aplicativo.
- Executavel simples em WinForms, sem dependencia de npm, Node.js ou instalador.

## Requisitos

- Windows.
- .NET Framework 4.x, ja presente na maioria das instalacoes do Windows.
- Para compilar: compilador C# do .NET Framework em `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` ou `Framework\v4.0.30319\csc.exe`.

## Versao

Versao atual: `1.0`.

## Uso

Baixe ou compile o executavel e abra:

```powershell
.\dist\MarkdownViewer.exe
```

Tambem e possivel passar um arquivo Markdown por argumento:

```powershell
.\dist\MarkdownViewer.exe .\examples\exemplo.md
```

No aplicativo:

- `Abrir .md`: seleciona um arquivo Markdown.
- `Recarregar`: recarrega o arquivo aberto do disco.
- `Tema escuro` / `Tema claro`: alterna o tema.
- `Modo codigo` / `Modo preview`: alterna entre o preview renderizado e o Markdown editavel.
- `Salvar`: grava alteracoes feitas no modo codigo.
- `Exportar PDF`: abre a impressao do Windows para salvar em PDF.

## Compilacao

Execute:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

O executavel sera gerado em:

```text
dist\MarkdownViewer.exe
```

## Estrutura

```text
src/                  Codigo fonte C#
scripts/              Scripts de build
assets/               Icone e recursos visuais do aplicativo
examples/             Arquivos Markdown de exemplo
docs/                 Documentacao tecnica e notas de publicacao
dist/                 Executaveis gerados localmente
```

## Observacoes

O aplicativo usa o controle `WebBrowser` do WinForms. Por isso, o HTML/CSS do preview foi mantido conservador para funcionar bem no motor nativo do Windows.

## Licenca

Este projeto esta disponivel sob a licenca MIT. Veja [LICENSE](LICENSE).
