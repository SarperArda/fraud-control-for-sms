"""
    Description: This script is used to train a machine learning model for SMS spam detection.
    Author: Sarper Arda BAKIR
    Date: 09-07-2024
    Version: 1.0
"""

import pandas as pd
import tensorflow as tf
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.preprocessing.sequence import pad_sequences
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Embedding, LSTM, Dense, Dropout
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report

# Step 1: Data Collection and Preprocessing
# Load dataset from CSV file
# Tiago Agostinho de Almeida and JosÈ MarÌa GÛmez Hidalgo hold the copyrigth (c) for the SMS Spam Collection v.1.
# The dataset is available at https://archive.ics.uci.edu/dataset/228/sms+spam+collection 
data = pd.read_csv('sms.csv', encoding='utf-8')
data = data[['label', 'message']]

# Text preprocessing
texts = data['message'].values
labels = data['label'].values

# Tokenization and padding
tokenizer = Tokenizer(num_words=5000)
tokenizer.fit_on_texts(texts)
sequences = tokenizer.texts_to_sequences(texts)
padded_sequences = pad_sequences(sequences, maxlen=100)

# Split data into training and testing sets
X_train, X_test, y_train, y_test = train_test_split(padded_sequences, labels, test_size=0.2, random_state=42)

# Step 2: Model Building
model = Sequential([
    Embedding(input_dim=5000, output_dim=64, input_length=100),
    LSTM(64, return_sequences=True),
    LSTM(32),
    Dropout(0.5),
    Dense(1, activation='sigmoid')
])

model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

# Step 3: Model Training
model.fit(X_train, y_train, epochs=10, batch_size=64, validation_split=0.2)

# Step 4: Model Evaluation
y_pred = (model.predict(X_test) > 0.5).astype("int32")
print(classification_report(y_test, y_pred))

# Step 5: Example Prediction
#new_sms = ["Congratulations! You've won a $1000 Walmart gift card. Go to http://bit.ly/12345 to claim now."]
new_sms = ["Hi dear, how are you doing today? Let's meet up for lunch."]
new_seq = tokenizer.texts_to_sequences(new_sms)
new_padded_seq = pad_sequences(new_seq, maxlen=100)
prediction = model.predict(new_padded_seq)

print("Fraud Probability:", prediction)
