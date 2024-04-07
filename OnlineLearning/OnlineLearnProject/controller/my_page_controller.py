from flask import Blueprint, render_template, session
from scripts.database import get_db

my_page = Blueprint('my_page', __name__, url_prefix='/my_page')

@my_page.route('/', methods=['GET','POST'])
def index():
    user_id = ""    
    if "user_id" in session:
        user_id = session.get("user_id")
    if user_id == "":
        pass
    sql = f"SELECT * FROM buy_online_learn WHERE user_id = ?"
    db = get_db()
    cursor = db.execute(sql, (user_id,))    
    buy_rows = cursor.fetchall()
    
    
    sql = f"SELECT * FROM sell_online_learn WHERE user_id = ?"
    db = get_db()
    cursor = db.execute(sql, (user_id,))    
    sell_rows = cursor.fetchall()
    
    cursor.close()
    db.close()
    
    return render_template('my_page.html',user_id=user_id, buy_rows = buy_rows, sell_rows = sell_rows)
