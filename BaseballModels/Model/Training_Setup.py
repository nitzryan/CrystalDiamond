from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base2_P','Base2_H','Base2')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly2_P','StatsOnly2_H','StatsOnly2')")
db.commit()