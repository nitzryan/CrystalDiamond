from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base_04JAN2026_P','Base_04JAN2026_H','Base_04JAN2026')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly_04JAN2026_P','StatsOnly_04JAN2026_H','StatsOnly_04JAN2026')")
db.commit()