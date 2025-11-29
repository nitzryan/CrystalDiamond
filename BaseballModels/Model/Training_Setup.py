from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base4_P','Base4_H','Base4')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly4_P','StatsOnly4_H','StatsOnly4')")
db.commit()