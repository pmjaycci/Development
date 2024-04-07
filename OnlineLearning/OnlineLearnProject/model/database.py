from flask import g
import sqlite3

DATABASE = 'database.db'


class Database:
    @staticmethod
    def get_db():
        if 'db' not in g:
            g.db = sqlite3.connect(DATABASE)
            g.db.row_factory = sqlite3.Row
        return g.db

    @staticmethod
    def close_db(e=None):
        db = g.pop('db', None)
        if db is not None:
            db.close()

    @staticmethod
    def init_db():
        db = Database.get_db()
        with open('schema.sql', mode='r') as f:
            db.executescript(f.read())
        db.commit()

    @staticmethod
    def write_db(sql, *values):
        db = Database.get_db()
        cursor = db.execute(sql, values)
        db.commit()
        cursor.close()

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
