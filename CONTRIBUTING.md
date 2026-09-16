# Contribuindo

Obrigado por considerar contribuir com o Visualizar Markdown.

## Ambiente local

1. Clone o repositorio.
2. Compile com:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

3. Teste abrindo:

```powershell
.\dist\MarkdownViewer.exe .\examples\exemplo.md
```

## Convencoes

- Mantenha o projeto sem dependencias externas sempre que possivel.
- Preserve compatibilidade com o `WebBrowser` do WinForms.
- Use CSS conservador no HTML renderizado.
- Atualize `CHANGELOG.md` quando adicionar recurso ou corrigir comportamento visivel.
- Nao versionar executaveis gerados; publique binarios pela area de Releases do GitHub.

## Checklist antes de enviar alteracao

- O projeto compila pelo `scripts/build.ps1`.
- O exemplo em `examples/exemplo.md` abre no preview.
- O modo codigo continua editavel e salva corretamente.
- A exportacao PDF continua abrindo a janela de impressao.
