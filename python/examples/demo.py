"""
Run with:  python3 examples/demo.py

Builds a policy, applies an endorsement, adds a declaration, saves it to
the (JSON-file-backed) document store, then reads the whole aggregate back
in a single call — no joins, no fan-out, no de-duplication.
"""
import os
import sys
from datetime import date

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "src"))

from document_model import Policy, Declaration, Endorsement, DocumentStore  # noqa: E402


def main():
    store_path = os.path.join(os.path.dirname(__file__), "_demo_store.json")
    store = DocumentStore(store_path)

    policy = Policy(
        policy_id="POL-2026-00042",
        policyholder="Northgate Logistics Ltd",
        inception_date=date(2026, 1, 1),
        base_premium=18_500.00,
    )

    policy.add_declaration(Declaration(
        declaration_id="DEC-1",
        description="Total insured value of warehouse contents",
        value="£2,400,000",
        effective_date=date(2026, 1, 1),
    ))

    policy.apply_endorsement(Endorsement(
        endorsement_id="END-1",
        description="Add second warehouse location (Leeds)",
        effective_date=date(2026, 4, 15),
        premium_adjustment=3_250.00,
    ))

    store.save(policy)
    print(f"Saved policy {policy.policy_id} as one document.\n")

    # --- The payoff: read the whole aggregate back in a single lookup ---
    reloaded = store.get(policy.policy_id)
    print("Read back in a single document fetch:")
    print(f"  Policyholder     : {reloaded.policyholder}")
    print(f"  Declarations     : {len(reloaded.declarations)}")
    print(f"  Endorsements     : {len(reloaded.endorsements)}")
    print(f"  Current premium  : £{reloaded.current_premium:,.2f}")
    print()
    print("Compare that to sql/relational_equivalent.sql, where the same")
    print("read needs a policies/declarations/endorsements join and the")
    print("application has to de-duplicate the fan-out itself.")

    os.remove(store_path)


if __name__ == "__main__":
    main()
