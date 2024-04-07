from flask import Flask
from OnlineLearnProject.controller.main_controller import main
from OnlineLearnProject.controller.login_controller import account
from OnlineLearnProject.controller.learn_detail_controller import learn_detail
from OnlineLearnProject.controller.learn_add_controller import learn_add
from OnlineLearnProject.controller.my_page_controller import my_page
from OnlineLearnProject.controller.payment_controller import payment

UPLOAD_FOLDER = 'static/uploads'

def create_app():
    app = Flask(__name__, template_folder='../templates', static_folder="../static")
    app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
    
    app.redirect_blueprint(main)
    app.redirect_blueprint(account)
    app.redirect_blueprint(learn_detail)
    app.redirect_blueprint(learn_add)
    app.redirect_blueprint(my_page)
    app.register_blueprint(payment)
    
    return app