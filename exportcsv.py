import pandas as pd

# Define the file path for the input text file and the output CSV file
input_file = 'sms.txt'  # Replace with your actual file path
output_file = 'sms.csv'  # Desired output CSV file

# Initialize lists to hold the labels and messages
labels = []
messages = []

# Read the input text file and process each line
with open(input_file, 'r', encoding='utf-8') as file:
    for line in file:
        # Split the line into label and message
        label, message = line.split('\t', 1)
        labels.append(label)
        messages.append(message.strip())  # Remove any leading/trailing whitespace

# Create a DataFrame from the collected data
data = pd.DataFrame({'label': labels, 'message': messages})

# Encode labels: 1 for spam, 0 for ham
data['label'] = data['label'].map({'spam': 1, 'ham': 0})

# Save the DataFrame to a CSV file
data.to_csv(output_file, index=False, encoding='utf-8')

print(f"Data has been successfully saved to {output_file}")
