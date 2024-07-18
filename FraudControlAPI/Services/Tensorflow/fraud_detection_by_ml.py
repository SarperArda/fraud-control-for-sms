"""
Description: This script is used to train a machine learning model for SMS spam detection.
Author: Sarper Arda BAKIR
Date: 09-07-2024
Version: 1.0
"""

import os
import sys
import pandas as pd
import tensorflow as tf
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.preprocessing.sequence import pad_sequences
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Embedding, LSTM, Dense, Dropout
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
import pickle

def train_sms_spam_model():
    # Step 1: Data Collection and Preprocessing
    data = pd.read_csv('sms.csv', encoding='utf-8')
    data = data[['label', 'message']]

    # Encode labels
    label_encoder = LabelEncoder()
    labels = label_encoder.fit_transform(data['label'].values)
    
    # Text preprocessing
    texts = data['message'].values

    # Tokenization and padding
    tokenizer = Tokenizer(num_words=5000)
    tokenizer.fit_on_texts(texts)
    sequences = tokenizer.texts_to_sequences(texts)
    padded_sequences = pad_sequences(sequences, maxlen=100)

    # Save the tokenizer
    with open('tokenizer.pickle', 'wb') as handle:
        pickle.dump(tokenizer, handle, protocol=pickle.HIGHEST_PROTOCOL)

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
    model.fit(X_train, y_train, epochs=100, batch_size=64, validation_split=0.2)

    # Save the trained model
    model.save('sms_spam_model.h5')

def predict_sms_spam(input_message):
    # Check if the model file exists
    if os.path.exists('sms_spam_model.h5') and os.path.exists('tokenizer.pickle'):
        # Load the trained model
        model = tf.keras.models.load_model('sms_spam_model.h5')

        # Load the tokenizer
        with open('tokenizer.pickle', 'rb') as handle:
            tokenizer = pickle.load(handle)

        # Tokenize and pad the input message
        sequence = tokenizer.texts_to_sequences([input_message])
        padded_sequence = pad_sequences(sequence, maxlen=100)

        # Make predictions
        prediction = model.predict(padded_sequence)[0][0]
        return prediction
    else:
        print("Model file or tokenizer not found. Please train the model first.")
        return None

if __name__ == "__main__":
    # Train the SMS spam detection model
    #train_sms_spam_model()
    if not os.path.exists('sms_spam_model.h5') and not os.path.exists('tokenizer.pickle'):
        train_sms_spam_model()
    if len(sys.argv) > 1:
        input_message = sys.argv[1]
        prediction = predict_sms_spam(input_message)
        if prediction is not None:
            print(f"Spam Probability: {prediction}")
    else:
        print("Please provide an input message for prediction.")