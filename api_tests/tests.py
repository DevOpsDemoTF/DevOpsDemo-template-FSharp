import unittest
import requests

class ApiTests(unittest.TestCase):
  def test_healthcheck(self):
    result = requests.get("http://app:8080/health")
    self.assertEqual(200, result.status_code)
