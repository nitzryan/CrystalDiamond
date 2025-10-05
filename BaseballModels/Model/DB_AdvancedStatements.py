from DBTypes import *
from typing import TypeVar, Type

_Left = TypeVar('L')
_Right = TypeVar('R')
def Select_LeftJoin(leftType : Type[_Left], rightType : Type[_Right], cursor : 'sqlite3.Cursor', query : str, conditions : tuple[any]) -> list[tuple[_Left, _Right]]:
    items = cursor.execute(query, conditions)
    return [
        (leftType(i[:leftType.NUM_ELEMENTS]), 
         rightType(
             tuple(0 if x is None else x for x in i[leftType.NUM_ELEMENTS:])
         )) for i in items
    ]

def Select_ModelHitterStats_LJ_PlayerMonthlyWar(cursor : 'sqlite3.Cursor', mlbId : int) -> list[tuple[DB_Model_HitterStats, DB_Player_MonthlyWar]]:
    items = cursor.execute('''SELECT *
                           FROM Model_HitterStats AS mhs
                           LEFT JOIN Player_MonthlyWar AS pmw
                            ON mhs.mlbId=pmw.mlbId AND mhs.month=pmw.month AND mhs.year=pmw.year
                            WHERE mhs.mlbId=? AND (isHitter=1 OR isHitter IS NULL)
                           ''', mlbId)
    
    return [
        (DB_Model_HitterStats(i[:DB_Model_HitterStats.NUM_ELEMENTS]), 
         DB_Player_MonthlyWar(
             tuple(0 if x is None else x for x in i[DB_Model_HitterStats.NUM_ELEMENTS:])
        )) 
        for i in items]