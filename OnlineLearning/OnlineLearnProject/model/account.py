
from flask import Blueprint, session, url_for
from model.database import Database

account = Blueprint('account', __name__, url_prefix='/account')


class Account:
    def login_check(id, pw):
        sql = "SELECT * FROM accounts WHERE user_id = ?"
        user = Database.read_once_db(sql, id)
        if user is not None:
            user_id = user['user_id']
            user_pw = user['pw']
            if id == user_id and pw == user_pw:
                print("로그인 성공")
            else:
                print("아이디 또는 패스워드 잘못됨")
        else:
            sql = f'INSERT INTO accounts (user_id, pw) VALUES(?,?)'
            Database.write_db(sql, id, pw)
            print("회원가입 성공")

        session['user_id'] = id
        return
