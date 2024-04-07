
from flask import Blueprint, redirect, request, session, url_for
from model.database import get_db

account = Blueprint('account', __name__, url_prefix='/account')


class Account:
    def login_check(id, pw):
        db = get_db()
        cursor = db.cursor()
        cursor.execute("SELECT * FROM accounts WHERE user_id = ?", (id,))
        user = cursor.fetchone()
        if user is not None:
            user_id = user['user_id']
            user_pw = user['pw']
            if id == user_id and pw == user_pw:
                print("로그인 성공")
            else:
                print("아이디 또는 패스워드 잘못됨")
        else:
            sql = f'INSERT INTO accounts (user_id, pw) VALUES(?,?)'
            cursor.execute(sql, (id, pw,))
            db.commit()
            print("회원가입 성공")

        cursor.close()
        db.close()
        session['user_id'] = id
        return
