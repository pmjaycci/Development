
from flask import g
import sqlite3

DATABASE = 'database.db'

class Database:
    # 데이터베이스 연결 함수
    @staticmethod
    def get_db():
        if 'db' not in g:
            g.db = sqlite3.connect(DATABASE)
            g.db.row_factory = sqlite3.Row
        return g.db
    @staticmethod
    # 데이터베이스 초기화 함수
    def init_db():
        db = Database.get_db()
        cursor = db.cursor()
        with open('schema.sql', mode='r') as f:
            cursor.executescript(f.read())
        db.commit()
        cursor.close()
        return
    
    @staticmethod
    def write_db(sql, *values):
        db = Database.get_db()
        cursor = db.execute(sql, values)
        db.commit()
    
        cursor.close()
        return
    
    @staticmethod
    def read_all_db(sql, *values):
        db = Database.get_db()
        cursor = db.execute(sql, values)
        rows = cursor.fetchall()
    
        cursor.close()
        return rows
    
    @staticmethod
    def read_once_db(sql, *values):
        db = Database.get_db()
        cursor = db.execute(sql, values)    
        row = cursor.fetchone()
    
        cursor.close()
        return row

