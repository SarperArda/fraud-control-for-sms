import matplotlib.pyplot as plt

# Verileri tanımla
data = {
    'Gemini': {'Doğru': 19, 'Yanlış': 11},
    'Tensorflow': {'Doğru': 22, 'Yanlış': 8},
    'IPQS': {'Doğru': 24, 'Yanlış': 6},
    'OpenAI': {'Doğru': 27, 'Yanlış': 3}
}

# Çubuk grafik verilerini ayır
categories = list(data.keys())
dogru = [data[cat]['Doğru'] for cat in categories]
yanlis = [data[cat]['Yanlış'] for cat in categories]

# Grafik oluşturma
fig, ax = plt.subplots()

bar_width = 0.35
index = range(len(categories))

bar1 = plt.bar(index, dogru, bar_width, label='Doğru')
bar2 = plt.bar([i + bar_width for i in index], yanlis, bar_width, label='Yanlış')

plt.xlabel('Kategoriler')
plt.ylabel('Sayılar')
plt.title('Kategorilere Göre Doğru ve Yanlış Sayıları')
plt.xticks([i + bar_width / 2 for i in index], categories)
plt.yticks(range(1, 31))
plt.legend()

plt.show()
