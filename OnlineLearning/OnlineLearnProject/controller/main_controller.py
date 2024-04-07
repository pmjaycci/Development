from flask import Blueprint, Flask, render_template, session
from model.main import Main

main = Blueprint('main', __name__)


@main.route('/')
def index():
    # 세션에서 사용자 이름을 가져옵니다.
    login_user = '로그인되지 않은 상태입니다.'
    is_login = False
    if "user_id" in session:
        login_user = f'안녕하세요, {session.get('user_id')}님!'
        is_login = True

    return render_template('index.html', login_user=login_user, is_login=is_login, rows=Main.learn_load())
