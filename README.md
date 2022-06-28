# Welcome to my book

Welcome to v2 of my book - "Develop Mobile Apps with C# and Azure".  This is a work in progress, with regular content updates.
Don't expect things to be accurate or stable as yet.

## Finding the book

You can find v1 of [the book online][book].  If you want to view v2, build the book.  Once the book is complete, I will publish it as the current site.

## Building the book

The book uses [MkDocs] with the [Material for Mkdocs] theme.  Install pre-requisites with the following:

```
pip install mkdocs-material mkdocs-git-revision-date-localized-plugin mkdocs-exclude mkdocs-awesome-pages-plugin
```

This will automatically install compatible versions of all dependencies: [Mkdocs], [Markdown], [Pygments], and [Python Markdown Extensions]. 

To build the static site, use:

```
mkdocs build --clean
```

To serve the static site locally, use:

```
mkdocs serve
```

The site will be on http://localhost:8000. 

<!-- Links -->
[book]: https://adrianhall.github.io/develop-mobile-apps-with-csharp-and-azure/
[Mkdocs]: https://www.mkdocs.org/
[Markdown]: https://python-markdown.github.io/
[Pygments]: https://pygments.org/
[Python Markdown Extensions]: https://facelessuser.github.io/pymdown-extensions/
[Material for Mkdocs]: https://squidfunk.github.io/mkdocs-material/
