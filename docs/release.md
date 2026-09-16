# Guia de publicacao no GitHub

## Primeira publicacao

```powershell
git init
git add README.md LICENSE CHANGELOG.md CONTRIBUTING.md .gitignore src scripts examples docs
git commit -m "Initial release"
git branch -M main
git remote add origin https://github.com/SEU_USUARIO/visualizar-markdown.git
git push -u origin main
```

## Gerar executavel para release

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

Depois, envie `dist\MarkdownViewer.exe` manualmente em uma Release do GitHub.

## Sugestao de tag

```powershell
git tag v1.0
git push origin v1.0
```

## Itens para conferir antes da release

- `README.md` descreve recursos atuais.
- `CHANGELOG.md` tem a versao correta.
- `scripts\build.ps1` compila sem erro.
- `examples\exemplo.md` abre no aplicativo.
- O executavel foi testado em uma maquina Windows.
