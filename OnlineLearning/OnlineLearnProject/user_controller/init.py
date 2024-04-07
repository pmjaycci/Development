from flask import Flask
from OnlineLearnProject.controller.main_controller import index
from scripts.login import login, logout
from scripts.learn_detail import *
from scripts.learn_add import *
from scripts.my_page import my_page
from scripts.payment import payment

UPLOAD_FOLDER = 'static/uploads'

def create_app():
    app = Flask(__name__, template_folder='../templates', static_folder="../static")
    app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
    
    app.route('/')(index)
    
    app.route('/learn_detail/<int:learn_id>')(learn_detail)
    app.route('/learn_detail_comment_post', methods=['POST'])(learn_add_comment_post)
    app.route('/learn_detail_comment_update', methods=['POST'])(learn_detail_comment_update)
    app.route('/learn_detail_comment_delete', methods=['POST'])(learn_detail_comment_delete)
    app.route('/learn_detail_comment_reply_post', methods=['POST'])(learn_detail_comment_reply_post)
    app.route('/learn_detail_comment_reply_update', methods=['POST'])(learn_detail_comment_reply_update)
    app.route('/learn_detail_comment_reply_delete', methods=['POST'])(learn_detail_comment_reply_delete)
    app.route('/learn_detail_buy', methods=['POST'])(learn_detail_buy)
    
    app.route('/login', methods=['POST'])(login)
    app.route('/logout', methods=['GET'])(logout)
    
    app.route('/learn_add',  methods=['POST', 'GET'])(learn_add)
    app.route('/learn_add_post',  methods=['POST'])(learn_add_post)
    app.route('/learn_add_post_test',  methods=['POST'])(learn_add_post_test)
    app.route('/upload_image', methods=['POST','GET'])(lambda: upload_image(app))
    app.route('/add_hash_tag', methods=['POST'])(add_hash_tag)
    
    app.route('/my_page', methods=['GET','POST'])(my_page)
    
    app.route('/pay', methods=['POST'])(payment)
    return app