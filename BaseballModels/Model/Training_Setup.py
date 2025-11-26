from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base3_P','Base3_H','Base3')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly2_P','StatsOnly2_H','StatsOnly2')")
db.commit()