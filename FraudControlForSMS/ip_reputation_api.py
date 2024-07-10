import json
import requests
import os
from dotenv import load_dotenv

class IPQS:
    load_dotenv()
    key = os.getenv("IP_QUALITY_SCORE_API_KEY")

    def payment_transaction_fraud_prev(self, ip: str, params: dict = {}) -> dict:
        """Method used to lookup Payment & Transaction Fraud Prevention API

        Args:
            ip (str): IP address to check
            params (dict, optional): Refer to https://www.ipqualityscore.com/documentation/proxy-detection-api/transaction-scoring for variables. Defaults to {}.

        Returns:
            dict: API response as a dictionary
        """
        if not params:
            return {}

        url = "https://www.ipqualityscore.com/api/json/ip/%s/%s" % (self.key, ip)
        response = requests.get(url, params=params)
        return json.loads(response.text)

def check_ip_fraud(ip_address: str):
    ipqs = IPQS()
    parameters = {
        'user_agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3',
        'user_language': 'en-US',
        'strictness': 0,
        # You may want to allow public access points like coffee shops, schools, corporations, etc...
        'allow_public_access_points': 'true',
        # Reduce scoring penalties for mixed quality IP addresses shared by good and bad users.
        'lighter_penalties': 'false'
    }

    result = ipqs.payment_transaction_fraud_prev(ip_address, parameters)

    if 'success' in result and result['success'] == True:
        if result['proxy'] == True or result['fraud_score'] >= 60:
            return True
        else:
            return False
    else:
        return False

'''
if __name__ == "__main__":
    # IP address to check
    ip_address = '99.86.4.119'
    result = check_ip_fraud(ip_address)
    print(result)
'''