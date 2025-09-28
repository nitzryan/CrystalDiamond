from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base_P','Base_H','Base')")
#cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly_P','StatsOnly_H','StatsOnly')")
db.commit()