"""
    Description: This script is used to interact with the Gemini API.
    Author: Sarper Arda BAKIR
    Date: 08-07-2024
    Version: 1.1
"""

# Import the required libraries
import os
import sys
from dotenv import load_dotenv
import google.generativeai as genai
from google.generativeai.types.generation_types import StopCandidateException

# Load the environment variables from the .env file
load_dotenv()

# Set the API key
API_KEY = os.getenv("GEMINI_API_KEY")

genai.configure(api_key=API_KEY)

"""
  This function sends a prompt to the Gemini AI API and returns the generated text response.

  Args:
      prompt: The user input string to be used as the prompt for Gemini AI.

  Returns:
      The generated text response from Gemini AI.
"""
def generate_gemini_response(prompt):
    try:
        # Create the model and chat session
        generation_config = {
            "temperature": 1,
            "top_p": 0.95,
            "top_k": 64,
            "max_output_tokens": 8192,
            "response_mime_type": "text/plain",
        }

        model = genai.GenerativeModel(
            model_name="gemini-1.5-flash",
            generation_config=generation_config,
        )

        chat_session = model.start_chat(
            history=[]
        )

        response = chat_session.send_message(prompt)
        return response

    except Exception as e:
        return f"An unexpected error occurred: {str(e)}"

if __name__ == "__main__":
    if len(sys.argv) > 1:
        prompt = sys.argv[1] + " Is this message fraud? Write only percentage of fraud."
        response = generate_gemini_response(prompt)
        print(response if isinstance(response, str) else response.text)
    else:
        print("Prompt is not provided. Please provide a prompt as an argument.")
