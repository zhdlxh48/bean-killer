import copy
import datetime
import enum
import flask
import flask.views
import flask_apscheduler
import flask_socketio
import json
import os
import pprint
import random
import secrets
import string
import sys
import time


user_no = 0
status = {}
color_palette = [
    '#ff0000',
    '#ffff00',
    '#00ff00',
    '#0000ff',
]


class GameDifficulty(enum.Enum):
    EASY = 0
    NORMAL = 1
    HARD = 2


def id_generator(size=6, chars=string.ascii_uppercase + string.digits):
   return ''.join(random.choice(chars) for _ in range(size))


def safe_int(data):
    try:
        return int(data)
    except Exception:
        return 0


########## HELPER FUNCTIONS ##########
def get_user_current_room(username):
    current_room = ''
    for room_id, room_status in status.items():
        if username in room_status['score']:
            current_room = room_id
            break

    return current_room


def get_active_user_list(target_room, status_dict=status):
    if not status_dict[target_room]['isPlaying']:
        # We must check user count only
        # because there can be a user that enters the room,
        # but isn't connected on socket
        return list(status_dict[target_room]['score'].keys())
    else:
        return [name for name, data in status_dict[target_room]['score'].items()
                if data['isSocketConnected']]


def get_left_color_in_room(target_room):
    left_color = copy.deepcopy(color_palette)
    for user_info in status[target_room]['score'].values():
        try:
            left_color.remove(user_info['color'])
        except Exception:
            pass
    return left_color


def create_user_info(target_user, target_room, entered_at):
    status[target_room]['score'][target_user] = {
        'user': target_user,
        'color': get_left_color_in_room(target_room)[0],
        
        'enteredAt': entered_at,
        'isSocketConnected': False,

        'playStartAgreed': False,
        
        'score': 0,
        'gameOver': False,
    }


def create_room(room_difficulty=GameDifficulty.NORMAL):
    global status
    id = id_generator()
    status[id] = {
        'id': id,
        'isPlaying': False,
        'difficulty': room_difficulty.value,
        'elapsed': 0,
        'score': {}
    }
    return id


def delete_room(id: str, status_dict=status):
    target_room = status_dict.pop(id, None)
    if target_room:
        for target_user in target_room['score']:
            # TODO REMOVE USER!!!
            pass
        socketio.close_room(id, '/play')


class Config:
    SECRET_KEY = secrets.token_hex(32)

    JOBS = [
        {
            'id': 'UserGC_1',
            'func': 'app:user_gc',
            'args': (status, ),
            'trigger': 'interval',
            'seconds': 2,
        },
    ]

    SCHEDULER_API_ENABLED = True


app = flask.Flask(__name__)
app.config.from_object(Config())
socketio = flask_socketio.SocketIO(app)


########## GARBAGE COLLECTOR FOR USER ##########
def user_gc(status_dict):

    copy_status_dict = copy.deepcopy(status_dict)

    for room_id, room_data in copy_status_dict.items():
        if not room_data['isPlaying']:
            for user_name, user_data in room_data['score'].items():
                if not user_data['isSocketConnected']:
                    entered_at_datetime = datetime.datetime.fromtimestamp(user_data['enteredAt'])
                    if entered_at_datetime + datetime.timedelta(seconds=6) < datetime.datetime.now():
                        print(f'User GC catched! {room_id}[{user_name}] = {entered_at_datetime + datetime.timedelta(seconds=6)}')
                        print(f'                 {datetime.datetime.now()}')
                        status_dict[room_id]['score'].pop(user_name, None)
                        socketio.emit('playerleave', {
                            'user': user_name,
                            'room': room_id
                        }, room=room_id, broadcast=True)

                        if not get_active_user_list(room_id, status_dict=status_dict):
                            print(f'Room GC catched! {room_id}')
                            delete_room(room_id, status_dict=status_dict)


scheduler = flask_apscheduler.APScheduler()
scheduler.init_app(app)
scheduler.start()


@app.before_request
def before_request():
    global user_no
    if 'session' in flask.session and 'username' in flask.session:
        pass
    else:
        print('########################################')
        print('####### NEW USER SESSION CREATED! ######')
        print('########################################')
        flask.session['session'] = os.urandom(24)
        flask.session['username'] = 'user'+str(user_no)
        user_no += 1


@app.route('/test')
def test_page():
    return (
        flask.send_from_directory('', 'index.html'),
        200,
        [
            # This is test webpage,
            # so Content-type must be text/html
            ('Content-type', 'text/html'),
            # This will force-reset User Session token
            # when requesting test webpage.
            ('Set-Cookie', 'session=DUMMY; max-age=0;'),
        ])


@app.route('/whoami')
def return_userid():
    try:
        target_user = flask.session['username']
        target_room = get_user_current_room(target_user)

        return flask.jsonify({
            'username': target_user,
            'color': status[target_room]['score'][target_user]['color'],
            'room': target_room,
            'difficulty': status[target_room]['difficulty']
        })
    except Exception:
        return flask.jsonify({
            'username': 'UNKNOWN',
            'color': '#000000',
            'room': 'UNKNOWN',
            'difficulty': GameDifficulty.HARD.value
        }), 400


class MainRoute(flask.views.MethodView):
    # ??? ????????? ?????? ??????
    # ?????? GET ???????????? ???
    def get(self):

        # dict?????? ????????? ordered??? ????????????
        # ??? ?????? ????????? ?????? ???????????????.
        # ?????? Python?????? dict()??? Python 3.7????????? ?????? ????????? ????????? ????????? ???????????????,
        # ?????? ???????????? ????????? ?????? ????????? ?????????????????????,
        # ??? ????????? ?????? ???????????? ?????? ?????? ????????? ??????.

        # ????????? ??? ?????? ?????? ?????? ?????? (???????????? ?????? / ?????? ?????? ???),
        # ????????? ??? ?????? ?????? ???????????? ????????????(???, ???????????? ????????? ?????? ??????) ????????? ?????? ???.
        room_list = list(status.keys())
        room_list.sort(
            key=lambda x: (status[x]['isPlaying'] or (len(status[x]['score']) >= 4)),
            reverse=False
        )

        return flask.jsonify({
            'success': True,
            'message': '??? ??????',
            'data': status,
            'sort': room_list,
        }), 201

    # ??? ?????? ??????
    # POST BODY??? 'room'??? ?????? ?????? ????????? ??? ID??? ??????
    def post(self):
        # ????????? ID??? ????????? ????????? ??? ????????? ?????????
        target_user = flask.session['username']
        target_room = flask.request.get_json(force=True)['room']
        print('status')
        print(status)

        # ?????? ?????????????????? ??????
        for room_id, room_status in status.items():
            if target_user in room_status['score']:
                return flask.jsonify({
                    'success': False,
                    'message': '?????? ?????? ?????????????????????!',
                }), 400

        # ?????? ????????? ????????? ????????? ??? ??????,
        # ?????? ???????????? 4??? ???????????? ???????????? ?????????
        room_status = status.get(target_room, None)
        if room_status:
            if (not room_status['isPlaying']) and (len(room_status['score']) < 4):
                create_user_info(
                    target_user,
                    target_room,
                    time.time())
                return flask.jsonify({
                    'success': True,
                    'message': '?????? ??????????????????!',
                }), 201
            elif room_status['isPlaying']:
                return flask.jsonify({
                    'success': False,
                    'message': '?????? ????????? ?????? ?????? ????????????!',
                }), 403
            elif len(room_status['score']) >= 4:
                return flask.jsonify({
                    'success': False,
                    'message': '????????? ?????? ????????????!',
                }), 403
        return flask.jsonify({
            'success': False,
            'message': '?????? ???????????? ????????????!',
        }), 404

    # ??? ??????
    def put(self):
        target_user = flask.session['username']
        try:
            target_difficulty_val = int(flask.request.get_json(force=True)['difficulty'])
            target_difficulty = GameDifficulty(target_difficulty_val)
        except Exception:
            target_difficulty = GameDifficulty.NORMAL

        for room_id, room_status in status.items():
            if target_user in room_status['score']:
                return flask.jsonify({
                    'success': False,
                    'message': '?????? ?????? ?????????????????????!',
                }), 400

        target_room = create_room(target_difficulty)
        create_user_info(
            target_user,
            target_room,
            time.time())
        return flask.jsonify({
            'success': True,
            'message': '?????? ?????????????????????!',
        }), 201

    # ????????? ????????? ?????? ????????? ????????? ????????? ?????????
    # TODO socket.io??? ????????? ??????????????? ???????????? ????????? ?????????
    def delete(self):
        target_user = flask.session['username']

        for room_id, room_status in status.items():
            room_status.pop(target_user, None)

        return flask.jsonify({
            'success': True,
            'message': '?????? ????????? ???????????????!'
        }), 205


app.add_url_rule('/', view_func=MainRoute.as_view('MainRoute'))


class PlayRoomSocket(flask_socketio.Namespace):
    def on_connect(self):
        target_user, target_room = '', ''
        try:
            target_user = flask.session['username']
            target_room = get_user_current_room(target_user)
        except Exception:
            print('Unexpected access to SocketIO, Kicking...')
            flask_socketio.emit('forcedisconnect', {
                'user': 'YOU!'
            }, room=None, broadcast=False)
            # flask_socketio.disconnect()
            return False

        if not target_room:
            # User is not in room, so kick out this user.
            flask_socketio.emit('forcedisconnect', {
                'user': 'YOU!'
            }, room=None, broadcast=False)
            # flask_socketio.disconnect()
            return False

        status[target_room]['score'][target_user]['isSocketConnected'] = True
        flask_socketio.join_room(target_room)

        flask_socketio.emit('playerenter', {
            'user': target_user,
            'room': target_room,
            'data': status[target_room]['score'][target_user],
        }, room=target_room, broadcast=True)

        for already_in_user, user_data in status[target_room]['score'].items():
            if already_in_user != target_user:
                flask_socketio.emit('playerenter', {
                    'user': already_in_user,
                    'room': target_room,
                    'data': status[target_room]['score'][already_in_user],
                }, room=None, broadcast=False)

    def on_disconnect(self):
        try:
            target_user = flask.session['username']
            target_room = get_user_current_room(target_user)

            if not status[target_room]['isPlaying']:
                flask_socketio.emit('playerleave', {
                    'user': target_user,
                    'room': target_room
                }, room=target_room, broadcast=True)

                status[target_room]['score'].pop(target_user, None)

            else:
                # ????????? ?????? ??? ????????? ?????? ????????? ?????????
                # ???, ?????? ????????? ???????????? ???????????? ?????? ????????? ????????? ?????? ????????????
                flask_socketio.emit('playergameover', {
                    'user': target_user,
                }, room=target_room, broadcast=True)

                status[target_room]['score'][target_user]['gameOver'] = True
                status[target_room]['score'][target_user]['isSocketConnected'] = False
                
                if all([z['gameOver'] for z in status[target_room]['score'].values()]):
                    result_score_sort = list(status[target_room]['score'].keys())
                    result_score_sort.sort(
                        key=lambda x: safe_int(status[target_room]['score'][x]['score']),
                        reverse=True
                    )

                    status[target_room]['isPlaying'] = False
                    status[target_room]['isGameOver'] = True
                    flask_socketio.emit('roomgameover', {
                        'user': target_user,
                        'data': status[target_room]['score'],
                        'sort': result_score_sort,
                    }, room=target_room, broadcast=True)

            flask_socketio.leave_room(target_room)

            if not get_active_user_list(target_room):
                delete_room(target_room)

            print(f'{target_user} disconnected!')
        except Exception:
            print(f'Unknown user disconnected!')
            pass

        flask.session.clear()

    # THIS METHOD SHOULD NOT BE USED IN PRODUCTION!!!
    def on_request(self, message):
        target_user = flask.session['username']
        target_room = get_user_current_room(target_user)

        flask_socketio.emit('response', {
            'data': message['data'],
            'user': target_user,
        }, room=target_room, broadcast=True)

    def on_playergetscore(self, message):
        target_user = flask.session['username']
        target_room = get_user_current_room(target_user)

        if type(message) is str:
            message = json.loads(message)

        status[target_room]['score'][target_user]['score'] = str(message.get('data', {}).get('score', 0))

        flask_socketio.emit('playergetscore', {
            'data': message['data'],
            'user': target_user,
        }, room=target_room, broadcast=True)

    def on_playerstartagree(self, message=''):
        target_user = flask.session['username']
        target_room = get_user_current_room(target_user)

        status[target_room]['score'][target_user]['playStartAgreed'] = True
        flask_socketio.emit('playerstartagree', {
            'user': target_user,
        }, room=target_room, broadcast=True)

        if all([z['playStartAgreed'] for z in status[target_room]['score'].values()]):
            status[target_room]['isPlaying'] = True
            flask_socketio.emit('roomstarted', {
                'user': target_user,
            }, room=target_room, broadcast=True)

    def on_playergameover(self, message=''):
        target_user = flask.session['username']
        target_room = get_user_current_room(target_user)

        status[target_room]['score'][target_user]['gameOver'] = True
        flask_socketio.emit('playergameover', {
            'user': target_user,
        }, room=target_room, broadcast=True)

        if all([z['gameOver'] for z in status[target_room]['score'].values()]):
            result_score_sort = list(status[target_room]['score'].keys())
            result_score_sort.sort(
                key=lambda x: safe_int(status[target_room]['score'][x]['score']),
                reverse=True
            )

            status[target_room]['isPlaying'] = False
            flask_socketio.emit('roomgameover', {
                'user': target_user,
                'data': status[target_room]['score'],
                'sort': result_score_sort,
            }, room=target_room, broadcast=True)


socketio.on_namespace(PlayRoomSocket('/play'))


########## THIS IS DUMMY ROUTE FOR TESTING! ##########
class DummyREST(flask.views.MethodView):
    def get(self):
        id_1, id_2, id_3 = id_generator(), id_generator(), id_generator()
        id_4, id_5, id_6 = id_generator(), id_generator(), id_generator()
        id_7, id_8, id_9 = id_generator(), id_generator(), id_generator()
        result = {
            id_1: {
                'id': id_1,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 0,
                'score': {},
            },
            id_2: {
                'id': id_2,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 1,
                'score': {
                    'user0': {
                        'user': 'user0',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#ff0000',
                    },
                    'user1': {
                        'user': 'user1',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#ffff00',
                    }
                },
            },
            id_3: {
                'id': id_3,
                'isPlaying': True,
                'elapsed': 0,
                'difficulty': 0,
                'score': {
                    'user2': {
                        'user': 'user2',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#ff0000',
                    },
                },
            },

            id_4: {
                'id': id_4,
                'isPlaying': False,
                'elapsed': 0,
                'score': {},
            },
            id_5: {
                'id': id_5,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 1,
                'score': {
                    'user0': {
                        'user': 'user0',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#ff0000',
                    },
                    'user1': {
                        'user': 'user1',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#ffff00',
                    },
                    'user2': {
                        'user': 'user2',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#00ff00',
                    },
                    'user3': {
                        'user': 'user3',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#0000ff',
                    }
                },
            },
            id_6: {
                'id': id_6,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 2,
                'score': {
                    'user2': {
                        'user': 'user2',
                        'playStartAgreed': False,
                        'score': 0,
                        'gameOver': False,
                        'color': '#ff0000',
                    },
                },
            },
            id_7: {
                'id': id_7,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 0,
                'score': {},
            },
            id_8: {
                'id': id_8,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 1,
                'score': {},
            },
            id_9: {
                'id': id_9,
                'isPlaying': False,
                'elapsed': 0,
                'difficulty': 2,
                'score': {},
            },
        }
        room_list = list(result.keys())
        room_list.sort(
            key=lambda x: (result[x]['isPlaying']
                           or (len(result[x]['score']) >= 4)),
            reverse=False
        )
        
        return flask.jsonify({
            'success': True,
            'data': result,
            'message': '??? ??????',
            'sort': room_list,
        }), 201


app.add_url_rule('/dummy', view_func=DummyREST.as_view('DummyListing'))

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=5000, debug=False)