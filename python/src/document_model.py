"""
Document-oriented domain model for an insurance Policy aggregate.

Policy -> Endorsements -> Declarations is naturally hierarchical, is almost
always retrieved as a whole (you rarely want "just the endorsements" without
the policy they belong to), and is updated far less often than it is read.
That shape maps cleanly onto a single document rather than three or four
normalised tables joined back together on every read.

This module has zero external dependencies on purpose: it should run
anywhere with a stock Python 3.9+ interpreter so it's trivial to clone,
read, and run.
"""
from __future__ import annotations

from dataclasses import dataclass, field, asdict
from datetime import date
from typing import List, Optional
import json


@dataclass
class Declaration:
    """A statement of fact made at inception or renewal (e.g. sums insured,
    named locations, risk questionnaire answers)."""
    declaration_id: str
    description: str
    value: str
    effective_date: date

    def to_dict(self) -> dict:
        d = asdict(self)
        d["effective_date"] = self.effective_date.isoformat()
        return d

    @staticmethod
    def from_dict(d: dict) -> "Declaration":
        d = dict(d)
        d["effective_date"] = date.fromisoformat(d["effective_date"])
        return Declaration(**d)


@dataclass
class Endorsement:
    """A change applied to a policy after inception (mid-term adjustment,
    coverage extension, correction, etc.)."""
    endorsement_id: str
    description: str
    effective_date: date
    premium_adjustment: float = 0.0

    def to_dict(self) -> dict:
        d = asdict(self)
        d["effective_date"] = self.effective_date.isoformat()
        return d

    @staticmethod
    def from_dict(d: dict) -> "Endorsement":
        d = dict(d)
        d["effective_date"] = date.fromisoformat(d["effective_date"])
        return Endorsement(**d)


@dataclass
class Policy:
    """The aggregate root. Everything that belongs to a policy travels with
    it as one document rather than being scattered across separate tables."""
    policy_id: str
    policyholder: str
    inception_date: date
    base_premium: float
    declarations: List[Declaration] = field(default_factory=list)
    endorsements: List[Endorsement] = field(default_factory=list)

    def add_declaration(self, declaration: Declaration) -> None:
        self.declarations.append(declaration)

    def apply_endorsement(self, endorsement: Endorsement) -> None:
        self.endorsements.append(endorsement)

    @property
    def current_premium(self) -> float:
        """The whole point of keeping endorsements with the policy: this
        number is trivial to compute from the document you already have,
        with no join required."""
        return self.base_premium + sum(e.premium_adjustment for e in self.endorsements)

    def to_document(self) -> dict:
        """Serialise the aggregate as a single self-contained document —
        this is what would be written to (and read back whole from) a
        document store such as Cosmos DB, MongoDB, or a JSON column."""
        return {
            "policy_id": self.policy_id,
            "policyholder": self.policyholder,
            "inception_date": self.inception_date.isoformat(),
            "base_premium": self.base_premium,
            "declarations": [d.to_dict() for d in self.declarations],
            "endorsements": [e.to_dict() for e in self.endorsements],
        }

    @staticmethod
    def from_document(doc: dict) -> "Policy":
        return Policy(
            policy_id=doc["policy_id"],
            policyholder=doc["policyholder"],
            inception_date=date.fromisoformat(doc["inception_date"]),
            base_premium=doc["base_premium"],
            declarations=[Declaration.from_dict(d) for d in doc.get("declarations", [])],
            endorsements=[Endorsement.from_dict(e) for e in doc.get("endorsements", [])],
        )


class DocumentStore:
    """A minimal stand-in for a real document database (Cosmos DB, MongoDB,
    a Postgres JSONB column, ...). It stores one JSON document per policy
    and retrieves the whole aggregate in a single read — no joins.

    Swap this class out for a real document DB client in production; the
    Policy model above doesn't change either way, which is the point.
    """

    def __init__(self, path: str):
        self._path = path
        try:
            with open(self._path, "r") as f:
                self._data = json.load(f)
        except (FileNotFoundError, json.JSONDecodeError):
            self._data = {}

    def save(self, policy: Policy) -> None:
        self._data[policy.policy_id] = policy.to_document()
        with open(self._path, "w") as f:
            json.dump(self._data, f, indent=2)

    def get(self, policy_id: str) -> Optional[Policy]:
        doc = self._data.get(policy_id)
        return Policy.from_document(doc) if doc else None

    def all_ids(self) -> List[str]:
        return list(self._data.keys())
