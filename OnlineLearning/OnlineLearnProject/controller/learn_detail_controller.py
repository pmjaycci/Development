from flask import Blueprint, redirect, render_template, request, session, url_for
from scripts.database import get_db

learn_detail = Blueprint('learn_detail', __name__, url_prefix='/learn_detail')

@learn_detail.route('/<int:learn_id>')(learn_detail)
def index(learn_id):
    # 세부 정보를 가져오는 코드
    is_login = False
    login_user = '로그인되지 않은 상태입니다.'
    
    sql = f"SELECT * FROM online_learn WHERE id = ?"
    db = get_db()
    cursor = db.execute(sql, (learn_id,))    
    learn_detail_row = cursor.fetchone()
    
    sql = f"SELECT * FROM online_learn_comment WHERE online_learn_id = {learn_id}"
    cursor = db.execute(sql)
    learn_detail_comment_rows = cursor.fetchall()    
    test_rows = []
    
    for row in learn_detail_comment_rows:
        sql = f"SELECT * FROM online_learn_reply WHERE comment_id = ?"
        #column_names = [description[0] for description in cursor.description]
        #print("Column names:", column_names)

        comment_id = row['id']
        cursor = db.execute(sql, (comment_id,))
        reply_rows = cursor.fetchall()
        combine_row = [row if row is not None else [], reply_rows if reply_rows is not None else [] ]
        test_rows.append(combine_row)

    user_id = ""    
    if "user_id" in session:
        is_login = True
        login_user = f'안녕하세요, {session.get("user_id")}님!'   
        user_id = session.get("user_id")
    cursor.close()
    db.close()  
    return render_template('learn_detail.html', learn_detail=learn_detail_row, comment_rows=test_rows, is_login=is_login, login_user=login_user, user_id = user_id)

    '''
       column_names = [description[0] for description in cursor.description]
       print("Column names:", column_names)
    '''

@learn_detail.route('/comment_post', methods=['POST'])
# 강의 내용 페이지 댓글 작성
def comment_post():
    if "user_id" not in session:
        return redirect(url_for('index'))
    
    learn_id = request.form.get('learn_id')
    user_id = session.get('user_id')
    comment = request.form.get('comment')

    sql = 'INSERT INTO online_learn_comment (online_learn_id, user_id, comment) VALUES (?,?,?)'
    db = get_db()
    cursor = db.cursor()
    cursor.execute(sql, (learn_id, user_id, comment))
    db.commit()
    
    cursor.close()
    db.close()
    
    print(f'learn_add_comment_post\n::댓글이 등록 되었습니다. 강의번호[{learn_id}] 작성자[{user_id}] 내용[{comment}]')
    return learn_detail(learn_id)

# 강의 내용 페이지 댓글 수정
@learn_detail.route('/comment_update', methods=['POST'])
def comment_update():
    if "user_id" not in session:
        return redirect(url_for('index'))
    learn_id = request.form.get('learn_id')
    comment_id = request.form.get('comment_id')
    comment = request.form.get('comment')
    user_id = session.get('user_id')

    db = get_db()
    
    sql = f"UPDATE online_learn_comment SET comment = ? WHERE id = ?"
    cursor = db.cursor()
    cursor.execute(sql, (comment, comment_id,))
    db.commit()
    
    
    cursor.close()
    db.close()
    
    print(f'learn_detail_comment_update\n::댓글이 업데이트 되었습니다. 강의번호[{learn_id}] 작성자[{user_id}] 삭제ID[{comment_id}]')
    return learn_detail(learn_id)

# 강의 내용 페이지 댓글 삭제
@learn_detail.route('/comment_delete', methods=['POST'])
def comment_delete():
    if "user_id" not in session:
        return redirect(url_for('index'))
    learn_id = request.form.get('learn_id')
    comment_id = request.form.get('comment_id')
    user_id = session.get('user_id')

    db = get_db()
    
    sql = f"DELETE FROM online_learn_comment WHERE id = ?"
    cursor = db.cursor()
    cursor.execute(sql, (comment_id,))
    db.commit()
    
    sql = f"DELETE FROM online_learn_reply WHERE comment_id = ?"
    cursor = db.cursor()
    cursor.execute(sql, (comment_id,))
    db.commit()
    
    cursor.close()
    db.close()
    
    print(f'learn_detail_comment_delete\n::댓글이 삭제 되었습니다. 강의번호[{learn_id}] 작성자[{user_id}] 삭제ID[{comment_id}]')
    return learn_detail(learn_id)

# 강의 내용 페이지 대댓글 작성
@learn_detail.route('/comment_reply_post', methods=['POST'])
def comment_reply_post():
    if "user_id" not in session:
        return redirect(url_for('index'))
    learn_id = request.form.get('learn_id')
    comment_id = request.form.get('comment_id')
    user_id = session.get('user_id')
    comment = request.form.get('comment')

    sql = 'INSERT INTO online_learn_reply (comment_id, user_id, comment) VALUES (?,?,?)'
    db = get_db()
    cursor = db.cursor()
    cursor.execute(sql, (comment_id, user_id, comment))
    db.commit()
    
    cursor.close()
    db.close()
    
    print(f'learn_add_comment_reply_post\n::대댓글이 등록 되었습니다. 강의번호[{learn_id}] 작성자[{user_id}] 내용[{comment}]')
    return learn_detail(learn_id)

# 강의 내용 페이지 대댓글 수정
@learn_detail.route('/comment_reply_update', methods=['POST'])
def comment_reply_update():
    if "user_id" not in session:
        return redirect(url_for('index'))
    learn_id = request.form.get('learn_id')
    comment_id = request.form.get('comment_id')
    comment = request.form.get('comment')
    user_id = session.get('user_id')

    db = get_db()
    
    sql = f"UPDATE online_learn_reply SET comment = ? WHERE id = ?"
    cursor = db.cursor()
    cursor.execute(sql, (comment, comment_id,))
    db.commit()
    
    
    cursor.close()
    db.close()
    
    print(f'learn_detail_comment_reply_update\n::대댓글이 업데이트 되었습니다. 강의번호[{learn_id}] 작성자[{user_id}] 삭제ID[{comment_id}]')
    return learn_detail(learn_id)

# 강의 내용 페이지 대댓글 삭제
@learn_detail.route('/comment_reply_delete', methods=['POST'])
def comment_reply_delete():
    if "user_id" not in session:
        return redirect(url_for('index'))
    learn_id = request.form.get('learn_id')
    delete_id = request.form.get('delete_id')
    user_id = session.get('user_id')

    sql = f"DELETE FROM online_learn_reply WHERE id = ?"
    db = get_db()
    cursor = db.cursor()
    cursor.execute(sql, (delete_id,))
    db.commit()
    
    cursor.close()
    db.close()
    
    print(f'learn_detail_comment_reply_delete\n::대댓글이 삭제 되었습니다. 강의번호[{learn_id}] 작성자[{user_id}] 삭제ID[{delete_id}]')
    return learn_detail(learn_id)

#강의 구매
@learn_detail.route('/buy', methods=['POST'])
def buy():
    if "user_id" not in session:
        return redirect(url_for('index'))
    learn_id = request.form.get('learn_id')
    delete_id = request.form.get('delete_id')
    user_id = session.get('user_id')

    sql = f"DELETE FROM online_learn_reply WHERE id = ?"
    db = get_db()
    cursor = db.cursor()
    cursor.execute(sql, (delete_id,))
    db.commit()
    
    cursor.close()
    db.close()
    
    print(f'learn_detail_buy\n::구매테스트. 강의번호[{learn_id}] 작성자[{user_id}] 삭제ID[{delete_id}]')
    return learn_detail(learn_id)
