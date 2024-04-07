import os
from flask import Blueprint, jsonify, redirect, render_template, request, session, url_for
from model.database import get_db
from werkzeug.utils import secure_filename
import uuid

learn_add = Blueprint('learn_add', __name__, url_prefix='/learn_add')


#region 강의 등록 페이지
@learn_add.route('/',  methods=['POST', 'GET'])
def index():
    if "user_id" not in session:
        return redirect(url_for('index'))

    is_login = True
    login_user = f'안녕하세요, {session.get('user_id')}님!'  
    hash_tag = ""   
    return render_template('learn_add.html', is_login = is_login, login_user = login_user, hash_tag = hash_tag)

@learn_add.route('/add_hash_tag', methods=['POST'])
def add_hash_tag():
    new_hash_tag = request.form.get('new_hash_tag')
    print(new_hash_tag)
    hash_tag = "뿜뿜"#hash_tag + new_hash_tag
    return redirect(url_for('learn_add'))#, hash_tag = hash_tag))
    #return redirect(url_for('index'))

#강의 등록 페이지 강의 등록
@learn_add.route('/learn_add_post',  methods=['POST'])
def learn_add_post():
    title = request.form.get('title')
    price = request.form.get('price')
    writer = session.get('user_id')#request.form.get('writer')
    content = request.form.get('content')
    sql = f'INSERT INTO online_learn (user_id, price, title, contents) VALUES(?,?,?,?)'
    db = get_db()
    
    cursor = db.execute(sql, (writer, price, title, content,))
    db.commit()
    
    cursor.close()
    db.close()
    return redirect(url_for('index'))

@learn_add.route('/learn_add_post_test',  methods=['POST'])
def learn_add_post_test():
    content = request.form['content']
    title = request.form.get('title')
    price = request.form.get('price')
    writer = session.get('user_id')#request.form.get('writer')
    sql = f'INSERT INTO online_learn (user_id, price, title, contents) VALUES(?,?,?,?)'
    db = get_db()
    
    cursor = db.execute(sql, (writer, price, title, content,))
    db.commit()
    
    cursor.close()
    db.close()
    return redirect(url_for('index'))


@learn_add.route('/upload_image', methods=['POST','GET'])
def upload_image():
    if 'upload' not in request.files:
        print("No file part in request")
        return redirect(request.url)
    image = request.files['upload']
    if image.filename == '':
        print("No selected file")
        return redirect(request.url)
    if image and allowed_file(image.filename):
        # UUID로 파일 이름 생성
        filename = image.filename#str(uuid.uuid4()) + '.' + image.filename.rsplit('.', 1)[1].lower()
        image.save(os.path.join(learn_add.config['UPLOAD_FOLDER'], secure_filename(filename)))
        print("File uploaded successfully:", filename)
        return jsonify({'uploaded': True, 'url': url_for('static', filename='uploads/' + filename)})
    print("Invalid file type")
    return redirect(request.url)


def allowed_file(filename):
    ALLOWED_EXTENSIONS = {'png', 'jpg', 'jpeg', 'gif', 'bmp', 'tiff'}
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS

'''
project
├─ app.py
├─ scripts
│       ├─ init.py
│       ├─ learn_add.py
│       ├─ learn_detail.py
│       ├─ login.py
│       ├─ main.py
│       ├─ my_page.py
│       └─ payment.py
│ 
├─ templates
│       ├─ index.html
│       ├─ learn_add.html
│       ├─ learn_detail.html
│       └─ my_page.html
│ 
└─ static
        └─ uploads
                └─ bart.jpg
'''
