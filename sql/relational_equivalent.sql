-- The same domain, modelled the "default" relational way.
--
-- This isn't wrong — it's a perfectly normal normalised schema. The point
-- of keeping it in this repo is to make the trade-off concrete: reading
-- back "everything about policy X" now costs three tables and two joins,
-- and every write that touches more than one table needs a transaction
-- to keep them consistent. Compare with the document model
-- (python/src/document_model.py or dotnet/src/DocumentModel/Policy.cs),
-- where the equivalent read is a single key lookup.

CREATE TABLE policies (
    policy_id       TEXT PRIMARY KEY,
    policyholder    TEXT NOT NULL,
    inception_date  DATE NOT NULL,
    base_premium    NUMERIC(12,2) NOT NULL
);

CREATE TABLE declarations (
    declaration_id   TEXT PRIMARY KEY,
    policy_id        TEXT NOT NULL REFERENCES policies(policy_id),
    description      TEXT NOT NULL,
    value            TEXT NOT NULL,
    effective_date   DATE NOT NULL
);

CREATE TABLE endorsements (
    endorsement_id     TEXT PRIMARY KEY,
    policy_id          TEXT NOT NULL REFERENCES policies(policy_id),
    description        TEXT NOT NULL,
    effective_date     DATE NOT NULL,
    premium_adjustment NUMERIC(12,2) NOT NULL DEFAULT 0
);

CREATE INDEX idx_declarations_policy_id  ON declarations(policy_id);
CREATE INDEX idx_endorsements_policy_id  ON endorsements(policy_id);

-- "Give me the whole policy" now looks like this:
--
-- SELECT p.*, d.*, e.*
-- FROM policies p
-- LEFT JOIN declarations  d ON d.policy_id = p.policy_id
-- LEFT JOIN endorsements  e ON e.policy_id = p.policy_id
-- WHERE p.policy_id = :policy_id;
--
-- ...and the application layer then has to de-duplicate the fan-out from
-- the double join before it can rebuild one policy object. Neither
-- version is "the right answer" in the abstract — the point of the
-- companion post is that the shape of the domain (hierarchical, read
-- together, written rarely) should decide which one you reach for.
