"""
    Description: This script is used to interact with the OpenAI API using the latest client library.
    Author: Sarper Arda BAKIR
    Date: 08-07-2024
    Version: 1.2
"""

# Import the required libraries
import os
import sys
from dotenv import load_dotenv
from openai import OpenAI

# Load the environment variables from the .env file
load_dotenv()

# Set the API key
API_KEY = os.getenv("OPENAI_API_KEY")

# Initialize OpenAI client
client = OpenAI(api_key=API_KEY)

"""
  This function sends a prompt to the OpenAI API and returns the generated text response.

  Args:
      prompt: The user input string to be used as the prompt for OpenAI.

  Returns:
      The generated text response from OpenAI.
"""
def generate_openai_response(prompt):
    try:
        # Adjust the prompt to include the specific question
        full_prompt = f"{prompt} Bu sms örneği Fraud mu? Sadece yüzdeyi yaz. Ayrıca, bu sms hangi kategoriye ait? Kampanya, Hukuki, OTP, Finans, Diğer.Vereceğin cevap yalnızca '%80 Kampanya' şeklinde olmalıdır.Fraud kelimesini ceavpta kullanma."

        # Create the completion request to OpenAI with streaming
        stream = client.chat.completions.create(
            model="gpt-4",  # Specify the model; adjust as needed
            messages=[{"role": "user", "content": full_prompt}],
            stream=True,
        )
        
        response_text = ""
        for chunk in stream:
            if chunk.choices[0].delta.content is not None:
                response_text += chunk.choices[0].delta.content
        
        return response_text.strip()

    except Exception as e:
        return f"An unexpected error occurred: {str(e)}"

if __name__ == "__main__":
    if len(sys.argv) > 1:
        test_message = sys.argv[1]
        result = generate_openai_response(test_message)
        print(f"Message: {test_message}")
        print(f"Response: {result}")
    else:
        print("Prompt is not provided. Please provide a prompt as an argument.")
'''
if __name__ == "__main__":
    test_message = "Değerli Müşterimiz; Koton Ak Yatırım Eş Liderliğinde halka arz ediliyor. 30 Nisan/ 2-3 Mayıs tarihlerinde 30,50 TL fiyat ile gerçekleşecek Koton halka arzına katılmak için hemen tıklayın. https://akyatirim.com.tr/halka-arz-talep SMS listesinden çıkmak için ILT IPTAL yazıp 4607'ye gönderebilirsiniz. Mersis:0011007783700015  B040"
    result = generate_openai_response(test_message)
    print(f"Message: {test_message}")
    print(f"Response: {result}")
'''
