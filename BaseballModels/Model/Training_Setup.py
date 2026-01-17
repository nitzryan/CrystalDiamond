from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base_16JAN2026_P','Base_16JAN2026_H','Base_16JAN2026')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly_16JAN2026_P','StatsOnly_16JAN2026_H','StatsOnly_16JAN2026')")
db.commit()