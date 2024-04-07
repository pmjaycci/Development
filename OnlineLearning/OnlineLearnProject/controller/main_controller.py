from flask import Blueprint, Flask, render_template, session
from model.database import get_db

main = Blueprint('main', __name__)

@main.route('/')
def index():
    # 세션에서 사용자 이름을 가져옵니다.
    login_user = '로그인되지 않은 상태입니다.'
    is_login = False
    if "user_id" in session:
        login_user = f'안녕하세요, {session.get('user_id')}님!'
        is_login = True
        
    return render_template('index.html', login_user = login_user, is_login = is_login, rows = learn_load())
        
def learn_load():
    sql = f'SELECT * FROM online_learn'
    db = get_db()
    cursor = db.cursor()
    cursor.execute(sql)
    rows = cursor.fetchall()

    cursor.close()
    db.close()
    return rows if rows is not None else []
