"""
Run with:  python3 -m unittest discover tests
"""
import os
import sys
import unittest
from datetime import date

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "src"))

from document_model import Policy, Declaration, Endorsement, DocumentStore  # noqa: E402


class TestPolicyAggregate(unittest.TestCase):
    def test_current_premium_includes_endorsements(self):
        policy = Policy("POL-1", "Test Ltd", date(2026, 1, 1), base_premium=1000.0)
        policy.apply_endorsement(Endorsement("END-1", "adjustment", date(2026, 2, 1), 250.0))
        policy.apply_endorsement(Endorsement("END-2", "adjustment", date(2026, 3, 1), -100.0))
        self.assertEqual(policy.current_premium, 1150.0)

    def test_round_trip_through_document(self):
        policy = Policy("POL-2", "Roundtrip Ltd", date(2026, 1, 1), base_premium=500.0)
        policy.add_declaration(Declaration("DEC-1", "sum insured", "£100,000", date(2026, 1, 1)))
        policy.apply_endorsement(Endorsement("END-1", "extension", date(2026, 5, 1), 50.0))

        doc = policy.to_document()
        rebuilt = Policy.from_document(doc)

        self.assertEqual(rebuilt.policy_id, policy.policy_id)
        self.assertEqual(len(rebuilt.declarations), 1)
        self.assertEqual(len(rebuilt.endorsements), 1)
        self.assertEqual(rebuilt.current_premium, policy.current_premium)

    def test_document_store_persists_whole_aggregate(self):
        tmp_path = os.path.join(os.path.dirname(__file__), "_test_store.json")
        if os.path.exists(tmp_path):
            os.remove(tmp_path)
        try:
            store = DocumentStore(tmp_path)
            policy = Policy("POL-3", "Store Ltd", date(2026, 1, 1), base_premium=750.0)
            policy.add_declaration(Declaration("DEC-1", "location", "London", date(2026, 1, 1)))
            store.save(policy)

            # New store instance simulates a fresh read from disk/DB.
            reloaded_store = DocumentStore(tmp_path)
            reloaded = reloaded_store.get("POL-3")

            self.assertIsNotNone(reloaded)
            self.assertEqual(reloaded.policyholder, "Store Ltd")
            self.assertEqual(len(reloaded.declarations), 1)
        finally:
            if os.path.exists(tmp_path):
                os.remove(tmp_path)


if __name__ == "__main__":
    unittest.main()
