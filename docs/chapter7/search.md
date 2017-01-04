# Integrating Mobile Search

Simple search capabilities can be handled by a little light LINQ usage and a small
search bar.  However, most catalog apps or video apps have search that seems almost
magical in what it produces.  It can handle multiple languages, bad spelling, the
difference between singular and plural spellings.  Once it has a set of matches, the
order of the results is based on relevance to your search query and the app can
highlight and provide additional search queries.

All of this is provided by external search services.  The most commonly used services
are based on [Lucene][1] which is an open-source text search engine library from the
Apache Project.  Azure Search is no exception here.  It provides a nice REST interface
that can be used by your app to provide search results.

In this chapters example, we are going to build something new and it doesn't use Azure
Mobile Apps much at all.  We are going to build a video search engine.  We will have a
number of videos that we will upload.  Those videos will be processed by the backend and
the audio and video content will be analyzed for searchable content.  Our app will be
able to search that content and get some matches back.

To start, I've created a simple Xamarin Forms app with a single view (called Search).
We'll update this app later on as we develop the code.  For now, the code for the app
is on the [GitHub repository][2].

## Configuring Azure Search

## Using Azure Search

<!-- Links -->
[1]: https://lucene.apache.org/
