# Model comparison

## Document-oriented (this repo's default)

```mermaid
graph TD
    P["Policy document<br/>POL-2026-00042"]
    P --> D1["declarations: [...]"]
    P --> E1["endorsements: [...]"]
    style P fill:#2563eb,color:#fff
```

One key, one read, the whole aggregate comes back together.

## Relational decomposition (`src/relational_equivalent.sql`)

```mermaid
graph TD
    Policies[(policies)] -->|policy_id FK| Declarations[(declarations)]
    Policies -->|policy_id FK| Endorsements[(endorsements)]
    Read["\"give me policy X\""] --> Policies
    Read --> Declarations
    Read --> Endorsements
    Declarations --> Dedup["app-layer de-duplication<br/>of the join fan-out"]
    Endorsements --> Dedup
```

Same information, three tables, a join, and a de-duplication step the
application has to own.

Neither shape is universally correct. The question this repo is trying to
make concrete is: *does this business entity behave like a document
(hierarchical, read as a whole, written rarely) or like a set (queried
across many records, updated independently, needing referential
integrity across a wide graph)?* Model to the shape of the domain, not to
habit.
