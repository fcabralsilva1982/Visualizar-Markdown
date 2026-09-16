# Arquitetura

## Visao geral

O Visualizar Markdown e um aplicativo WinForms de arquivo unico. A classe `ViewerForm` gerencia a interface e o controle `WebBrowser`; a classe `Markdown` transforma texto Markdown em HTML.

## Decisoes tecnicas

### WinForms e .NET Framework

O projeto usa WinForms para gerar um executavel simples e compativel com Windows sem exigir runtime moderno, Node.js ou instalador.

### Renderizacao via WebBrowser

O preview e exibido no controle `WebBrowser`. Como esse controle usa um motor antigo do Windows, o HTML/CSS gerado evita recursos modernos que podem falhar em algumas maquinas.

### Parser Markdown interno

O parser cobre os elementos principais usados em documentacao operacional:

- Titulos.
- Paragrafos.
- Listas ordenadas e nao ordenadas.
- Blocos de citacao.
- Tabelas simples.
- Imagens e links.
- Codigo inline.
- Blocos de codigo com linguagem.

Ele nao pretende substituir parsers completos como Markdig. A escolha reduz dependencias e simplifica distribuicao.

### Exportacao PDF

A exportacao usa a janela de impressao do Windows. Para gerar PDF, selecione `Microsoft Print to PDF`.

## Limitacoes conhecidas

- Markdown avancado, como listas aninhadas complexas, HTML misturado e extensoes especificas de GitHub, pode renderizar parcialmente.
- O realce de sintaxe e simples e baseado em expressoes regulares.
- A exportacao PDF depende das impressoras configuradas no Windows.
