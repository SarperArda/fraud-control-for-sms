import json
import requests
import urllib
import os
from dotenv import load_dotenv
from ip_reputation_api import check_ip_fraud


class IPQS:
    load_dotenv()
    key = os.getenv("IP_QUALITY_SCORE_API_KEY")
    
    def malicious_url_scanner_api(self, url: str, vars: dict = {}) -> dict:
        url = 'https://www.ipqualityscore.com/api/json/url/%s/%s' % (self.key, urllib.parse.quote_plus(url))
        x = requests.get(url, params=vars)
        return json.loads(x.text)

if __name__ == "__main__":
    """
    URL to scan - URL Encoded in cURL function below.
    """
    URL = 'https://f1store.formula1.com/en/'

    strictness = 0
    timeout = 5

    # Custom fields
    additional_params = {
        'strictness': strictness,
        'timeout': timeout
    }

    ipqs = IPQS()
    result = ipqs.malicious_url_scanner_api(URL, additional_params)

    if 'success' in result and result['success'] == True:
        if result['suspicious'] == True or result['phishing'] == True or result['malware'] == True or result['risk_score'] > 75 or result['parking'] == True:
            print("fraud website")

        else:
            ip_address = result['ip_address']
            result = check_ip_fraud(ip_address)
            if result:
                print("fraud website")
            else:
                print("legitimate website")    
    else:
        print("unsuccessful request")
