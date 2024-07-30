import matplotlib.pyplot as plt

# Verileri tanımla
data1 = {'Low': 2, 'Moderate': 2, 'Fraud': 11}
data2 = {'Low': 14, 'Moderate': 1}

# Pasta grafiği için verileri hazırla
labels1 = list(data1.keys())
sizes1 = list(data1.values())
colors1 = ['gold', 'yellowgreen', 'lightcoral']

labels2 = list(data2.keys())
sizes2 = list(data2.values())
colors2 = ['gold', 'yellowgreen']

# Pasta grafiği oluşturma
fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(14, 7))

# İlk pasta grafiği
ax1.pie(sizes1, labels=labels1, colors=colors1, autopct='%1.1f%%', startangle=140)
ax1.axis('equal')
ax1.set_title('Fraud SMS\'lerin Dağılımı')

# İkinci pasta grafiği
ax2.pie(sizes2, labels=labels2, colors=colors2, autopct='%1.1f%%', startangle=140)
ax2.axis('equal')
ax2.set_title('Fraud Olmayan SMS\'lerin Dağılımı')

plt.show()
