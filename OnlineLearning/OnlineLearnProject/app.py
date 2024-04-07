from scripts.database import init_db
from scripts.init import create_app

app = create_app()

app.secret_key = 'your_secret_key'  # 이 부분에 적절한 시크릿 키를 입력하세요.

if __name__ == '__main__':
    init_db()
    app.run(debug=True)
