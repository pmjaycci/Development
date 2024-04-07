from werkzeug.utils import secure_filename
import uuid


class LearnAdd:
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
