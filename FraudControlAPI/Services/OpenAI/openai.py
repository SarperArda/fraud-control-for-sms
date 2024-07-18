import openai
import os
from dotenv import load_dotenv

load_dotenv()
API_KEY = os.getenv("OPENAI_API_KEY")

def is_fraudulent(message):
    openai.api_key = API_KEY
    
    response = openai.Completion.create(
        engine="text-davinci-003",
        prompt=f"Is the following message fraudulent?\n\nMessage: {message}\n\nResponse:",
        max_tokens=50,
        n=1,
        stop=None,
        temperature=0.5,
    )
    
    result = response.choices[0].text.strip()
    return result

if __name__ == "__main__":
    test_message = "Congratulations! You've won a free cruise. Call this number to claim your prize."
    result = is_fraudulent(test_message)
    print(f"Message: {test_message}")
    print(f"Fraudulent: {result}")
