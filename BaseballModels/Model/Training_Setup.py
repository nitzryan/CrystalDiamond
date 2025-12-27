from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base_27DEC2025_P','Base_27DEC2025_H','Base_27DEC2025')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly_27DEC2025_P','StatsOnly_27DEC2025_H','StatsOnly_27DEC2025')")
db.commit()