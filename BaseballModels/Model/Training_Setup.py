from Constants import db

cursor = db.cursor()
cursor.execute("DELETE FROM ModelIdx")
db.commit()

cursor = db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base5_P','Base5_H','Base5')")
cursor.execute("INSERT INTO ModelIdx VALUES(2,'StatsOnly5_P','StatsOnly5_H','StatsOnly5')")
cursor.execute("INSERT INTO ModelIdx VALUES(3,'Experimental5_P','Experimental5_H','Experimental5')")
db.commit()