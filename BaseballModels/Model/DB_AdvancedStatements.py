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
