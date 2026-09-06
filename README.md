# Insurance Domain: Document Model vs Relational Decomposition

A small, runnable companion to a LinkedIn post on when a business entity
should be modelled as a document rather than decomposed into relational
tables — using an insurance Policy/Endorsement/Declaration aggregate as
the working example.

> *"Not every problem needs a relational database... The interesting
> question isn't SQL or NoSQL — it's what shape does the business entity
> actually have."*
> — [companion post](#) <!-- swap in the LinkedIn post URL -->

## The idea

A Policy, its Endorsements, and its Declarations are:

- **Naturally hierarchical** — endorsements and declarations only make
  sense in the context of the policy they belong to.
- **Retrieved together** — you almost never want "just the endorsements"
  without the policy.
- **Updated infrequently relative to reads** — a policy might get a
  handful of endorsements a year but gets read constantly (quoting,
  servicing, claims, reporting).

That shape maps naturally onto a single document. This repo shows both
versions side by side so the trade-off is concrete rather than abstract,
implemented twice — once in Python, once in C# — since the pattern is
about domain modelling, not language:

| | Document model | Relational equivalent |
|---|---|---|
| Storage shape | One document per policy | Three normalised tables |
| "Read the whole policy" | Single key lookup | Two joins + app-layer de-dup |
| Referential integrity | Enforced by the aggregate | Enforced by foreign keys |
| Best suited to | Hierarchical, read-heavy aggregates | Data queried/joined across many unrelated entities |

See [`diagrams/model-comparison.md`](diagrams/model-comparison.md) for a
visual version of the same comparison.

## Running it

**Python** — no external dependencies, stock Python 3.9+ only:

```bash
python3 python/examples/demo.py          # builds, saves, and re-reads a policy
python3 -m unittest discover python/tests
```

**C#** — .NET 8 SDK, no NuGet packages required (fully offline-restorable):

```bash
cd dotnet
dotnet run --project examples/Demo/Demo.csproj      # same walkthrough as the Python demo
dotnet run --project tests/DocumentModel.Tests/DocumentModel.Tests.csproj
# or build/test everything via the solution:
dotnet build InsuranceDomainDocumentModel.sln
```

## Repo layout

```
sql/
  relational_equivalent.sql   # the same domain, normalised (language-agnostic)
python/
  src/document_model.py        # Policy aggregate + minimal document-store stand-in
  examples/demo.py             # runnable end-to-end example
  tests/test_document_model.py
dotnet/
  InsuranceDomainDocumentModel.sln
  src/DocumentModel/           # same aggregate, as a .NET class library
  examples/Demo/               # same walkthrough, as a console app
  tests/DocumentModel.Tests/   # dependency-free test runner (no xUnit/NuGet needed)
diagrams/
  model-comparison.md
```

Both implementations model exactly the same aggregate and produce the
same demo output — pick whichever matches the stack you're pitching to,
or read both to see the pattern is the point, not the syntax.

## Where this doesn't apply

This pattern is about aggregates that behave like documents. Data that's
queried and joined across many unrelated records — counterparty
reference data, trade positions sliced by book/portfolio/trader — is
usually still better served by a relational or columnar model. The point
isn't "documents are better," it's "let the shape of the domain pick the
storage model, not habit."

## License

MIT — see [LICENSE](LICENSE).
