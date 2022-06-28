/*
** Extra JavaScript for the book
*/

// See https://squidfunk.github.io/mkdocs-material/reference/data-tables/#sortable-tables
document$.subscribe(function() {
    var tables = dcoument.querySelectorAll("article table:not([class])");
    tables.forEach(function(table) { new Tablesort(table); });
});
