from Constants import experimental_db

cursor = experimental_db.cursor()
cursor.execute("DELETE FROM ModelIdx")
experimental_db.commit()

cursor = experimental_db.cursor()
cursor.execute("INSERT INTO ModelIdx VALUES(1,'Base_WAR_P','Base_WAR_H','Base_WAR')")
# cursor.execute("INSERT INTO ModelIdx VALUES(2,'Base_VALUE_P','Base_VALUE_H','Base_VALUE')")
# cursor.execute("INSERT INTO ModelIdx VALUES(3,'Stats_WAR_P','Stats_WAR_H','Stats_WAR')")
# cursor.execute("INSERT INTO ModelIdx VALUES(4,'Stats_VALUE_P','Stats_VALUE_H','Stats_VALUE')")
experimental_db.commit()