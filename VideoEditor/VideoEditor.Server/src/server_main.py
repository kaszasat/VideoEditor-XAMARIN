from importlib.metadata import files
from flask import Flask, Blueprint, request, send_file, flash
from src.logger import LOG
import os
import json
import subprocess
import string, random
import atexit

server_api = Blueprint('Server', __name__)
app = Flask(__name__)
globbal_id = 0

def Start():
    atexit.register(onclose)
    UPLOAD_FOLDER = os.getcwd()
    print(" * Upload folder: " + UPLOAD_FOLDER)
    app.config['UPLOAD_FOLDER'] = UPLOAD_FOLDER
    app.register_blueprint(server_api)
    app.run(host='0.0.0.0', port=5000, debug=True)

def onclose():
    filelist = [ f for f in os.listdir(os.getcwd()) 
                if f.endswith(".mp4") or f.endswith(".psd") or f.endswith(".mp3") or f.endswith(".eps") or f.endswith(".ai") or f.endswith(".wav")
                or f.endswith(".mkv") or f.endswith(".mov") or f.endswith(".wmv") or f.endswith(".avi") or f.endswith(".avchd") or f.endswith(".flv")
                or f.endswith(".jpg") or f.endswith(".f4v") or f.endswith(".swf") or f.endswith(".webm") or f.endswith(".gif") or f.endswith(".tiff")
                or f.endswith(".bmp") or f.endswith(".flac") or f.endswith(".aac") or f.endswith(".alac") or f.endswith(".aiff") or f.endswith(".dsd") ]
    for f in filelist:
        os.remove(os.path.join(os.getcwd(), f))

@server_api.route('/trim', methods=['POST'])
def trim_video_then_respond():
    global globbal_id
    LOG.debug(f'{globbal_id} - Request Received - /trim')
    globbal_id+=int(1)
    
    if 'file' not in request.files:
        flash('No file part')
        LOG.debug('Request contained no file.')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}

    for x in request.files.getlist('file'):
        if x.filename.strip().replace('"', '') == "command.txt":
            command = x
        else:
            file_to_be_trimmed = x
            if file_to_be_trimmed == '':
                flash('No selected file')
                LOG.debug('Request filename empty.')
                return json.dumps({'success':False}), 204, {'ContentType':'application/json'}

    filename = ''.join(random.sample(string.ascii_lowercase, 7)) + file_to_be_trimmed.filename.strip().replace('"', '')
    LOG.debug(f'Saving file: {filename}')
    file_to_be_trimmed.save(os.path.join(app.config['UPLOAD_FOLDER'], filename))
    commandstring = command.read().decode("utf-8")
    if ";" in commandstring:
        flash('Text contains illegal character')
        LOG.debug('Text contains illegal character')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}

    outputfilename = f"{globbal_id}_trim_output.mp4"
    LOG.debug("Accompanying command string for trimming: "+ commandstring)
    cmd = f'ffmpeg -y -i "{filename}" {commandstring} {outputfilename}'
    
    subprocess.run(cmd, shell=True)

    

    return send_file(os.path.join(app.config['UPLOAD_FOLDER'], outputfilename))

@server_api.route('/rearrange', methods=['POST'])
def rearrange_video_then_respond():
    global globbal_id
    
    LOG.debug(f'{globbal_id} - Request Received - /rearrange')
    globbal_id+=int(1)
    
    
    if 'file' not in request.files:
        flash('No file part')
        LOG.debug('Request contained no file.')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}

    filestoberearranged = list()
    inputcommandparamteres = ""
    conversionparameters = ""
    endofconversionparameters = ""
    counter=int(0)

    for x in request.files.getlist('file'):
        file_to_be_rearranged = x
        if file_to_be_rearranged == '':
            flash('No selected file')
            LOG.debug('Request filename empty.')
            return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
        else:
            filename = ''.join(random.sample(string.ascii_lowercase, 7)) + file_to_be_rearranged.filename.strip().replace('"', '')
            LOG.debug(f'Saving file: {filename}')
            file_to_be_rearranged.save(os.path.join(app.config['UPLOAD_FOLDER'], filename))
            filestoberearranged.append(filename)
            inputcommandparamteres += f' -i "{filename}" '
            conversionparameters += (f"[{counter}:v]scale=1920:1080:force_original_aspect_ratio=decrease,"+
                                     f"pad=1920:1080:-1:-1,setsar=1,fps=30,format=yuv420p[v{counter}];")
            endofconversionparameters += f"[v{counter}][{counter}:a]"
            counter+=int(1)

             
    outputfilename = f"{globbal_id}_rearrange_output.mp4"
    cmd = (f'ffmpeg -y {inputcommandparamteres} -filter_complex "{conversionparameters}{endofconversionparameters}concat=n={len(filestoberearranged)}'+
    f':v=1:a=1[v][a]" -map "[v]" -map "[a]" -c:v libx264 -c:a aac -movflags +faststart "{outputfilename}"')

    #if ";" in cmd:
    #    flash('Text contains illegal character')
    #    LOG.debug('Text contains illegal character')
    #    return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
    LOG.debug(cmd)
    subprocess.run(cmd, shell=True)



    return send_file(os.path.join(app.config['UPLOAD_FOLDER'], outputfilename))


@server_api.route('/montage', methods=['POST'])
def montage_then_respond():
    global globbal_id
    LOG.debug(f'{globbal_id} - Request Received - /montage')
    globbal_id+=int(1)
    
    if 'file' not in request.files:
        flash('No file part')
        LOG.debug('Request contained no file.')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}

    filestobemontaged = list()
    inputcommandparamteres = ""
  
    for x in request.files.getlist('file'):
        file_to_be_montaged = x
        if file_to_be_montaged == '':
            flash('No selected file')
            LOG.debug('Request filename empty.')
            return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
        else:
            filename = ''.join(random.sample(string.ascii_lowercase, 7)) + file_to_be_montaged.filename.strip().replace('"', '')
            LOG.debug(f'Saving file: {filename}')
            file_to_be_montaged.save(os.path.join(app.config['UPLOAD_FOLDER'], filename))
            filestobemontaged.append(filename)
            inputcommandparamteres += f' -loop 1 -t 3 -i "{filename}" ' 

             
    outputfilename = f"{globbal_id}_montage_output.mp4"
    cmd = (f'ffmpeg -y {inputcommandparamteres} -f lavfi -i anullsrc -filter_complex "concat=n={len(filestobemontaged)}:'+
           f'v=1:a=0:unsafe=1,scale=1920:1080:force_original_aspect_ratio=decrease:eval=frame,pad=1920:1080:-1:-1:eval=frame"'+
           f' -c:v libx264 -pix_fmt yuv420p -r 25 -movflags +faststart -shortest {outputfilename} -loglevel panic')

    if ";" in cmd:
        flash('Text contains illegal character')
        LOG.debug('Text contains illegal character')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
    LOG.debug(cmd)
    subprocess.run(cmd, shell=True)



    return send_file(os.path.join(app.config['UPLOAD_FOLDER'], outputfilename))

@server_api.route('/addeffecttovideo', methods=['POST'])
def add_effect_to_video_then_respond():
    global globbal_id
    LOG.debug(f'{globbal_id} - Request Received - /addeffecttovideo')
    globbal_id+=int(1)

    if 'file' not in request.files:
        flash('No file part')
        LOG.debug('Request contained no file.')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
    effectstring = ""
    lowerValue = ""
    uppervalue = ""

    for x in request.files.getlist('file'):
        if x.filename.strip().replace('"', '') == "command.txt":
            command = x.read().decode("utf-8")
            effectstring = command.split("\n")[0]
            lowerValue = command.split("\n")[1]
            uppervalue = command.split("\n")[2]
        else:
            file_to_be_added_effect = x
            if file_to_be_added_effect == '':
                flash('No selected file')
                LOG.debug('Request filename empty.')
                return json.dumps({'success':False}), 204, {'ContentType':'application/json'}


    filePath = ''.join(random.sample(string.ascii_lowercase, 7)) + file_to_be_added_effect.filename.strip().replace('"', '')
    LOG.debug(f'Saving file: {filePath}')
    file_to_be_added_effect.save(os.path.join(app.config['UPLOAD_FOLDER'], filePath))
    
    destination = f"{globbal_id}_effect_output.mp4"

    if effectstring =="Black and White":
        cmd=(f'ffmpeg -y -i "{filePath}" -vf "split=3[pre][affected][post];' +
                        f'[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];' +
                        f'[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS,eq=contrast=250.0[affected];' +
                        f'[post]trim={uppervalue},setpts=PTS-STARTPTS[post];' +
                        f'[pre][affected][post]concat=n=3:v=1:a=0" ' +
                        f'"{destination}"')
    elif effectstring == "Grayscale":
        cmd=(f'ffmpeg -y -i "{filePath}" ' +
                        f'-vf "split=3[pre][affected][post];' +
                        f'[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];' +
                        f'[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS,' +
                        f'colorchannelmixer=.3:.4:.3:0:.3:.4:.3:0:.3:.4:.3[affected];' +
                        f'[post]trim={uppervalue},setpts=PTS-STARTPTS[post];' +
                        f'[pre][affected][post]concat=n=3:v=1:a=0"' +
                        f' "{destination}"')
    elif effectstring == "Sepia":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"colorchannelmixer=.393:.769:.189:0:.349:.686:.168:0:.272:.534:.131[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Darker":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"colorlevels=rimin=0.058:gimin=0.058:bimin=0.058[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Increased contrast":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"colorlevels=rimin=0.039:gimin=0.039:bimin=0.039:rimax=0.96:gimax=0.96:bimax=0.96[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Lighter":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"colorlevels=rimax=0.902:gimax=0.902:bimax=0.902[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Increased brightness":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"colorlevels=romin=0.5:gomin=0.5:bomin=0.5[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Sharpen":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"convolution='0 -1 0 -1 5 -1 0 -1 0:0 -1 0 -1 5 -1 0 -1 0:0 -1 0 -1 5 -1 0 -1 0:0 -1 0 -1 5 -1 0 -1 0'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Laplacian edge detector which includes diagonals":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"convolution='1 1 1 1 -8 1 1 1 1:1 1 1 1 -8 1 1 1 1:1 1 1 1 -8 1 1 1 1:1 1 1 1 -8 1 1 1 1:5:5:5:1:0:128:128:0'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring ==  "Blur":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"convolution='1 1 1 1 1 1 1 1 1:1 1 1 1 1 1 1 1 1:1 1 1 1 1 1 1 1 1:1 1 1 1 1 1 1 1 1:1/9:1/9:1/9:1/9'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Edge enhance":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"convolution='0 0 0 -1 1 0 0 0 0:0 0 0 -1 1 0 0 0 0:0 0 0 -1 1 0 0 0 0:0 0 0 -1 1 0 0 0 0:5:1:1:1:0:128:128:128'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Edge detect":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"convolution='0 1 0 1 -4 1 0 1 0:0 1 0 1 -4 1 0 1 0:0 1 0 1 -4 1 0 1 0:0 1 0 1 -4 1 0 1 0:5:5:5:1:0:128:128:128'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Emboss":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"convolution='-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Red color cast to shadows":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"colorbalance=rs=.3[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Slightly increased middle level of blue":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"curves=blue='0/0 0.5/0.58 1/1'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Vintage effect":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"curves=r='0/0.11 .42/.51 1/0.95':g='0/0 0.50/0.48 1/1':b='0/0.22 .49/.44 1/0.8'[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Denoise with a sigma of 4.5":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"dctdnoiz=4.5[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Weak deblock":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"deblock=filter=weak:block=4[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    elif effectstring == "Strong deblock":
        cmd=(f"ffmpeg -y -i \"{filePath}\" " +
                        f"-vf \"split=3[pre][affected][post];" +
                        f"[pre]trim=0:{lowerValue},setpts=PTS-STARTPTS[pre];" +
                        f"[affected]trim={lowerValue}:{uppervalue},setpts=PTS-STARTPTS," +
                        f"deblock=filter=strong:block=4:alpha=0.12:beta=0.07:gamma=0.06:delta=0.05[affected];" +
                        f"[post]trim={uppervalue},setpts=PTS-STARTPTS[post];" +
                        f"[pre][affected][post]concat=n=3:v=1:a=0\"" +
                        f" \"{destination}\"")
    #if ";" in cmd:
    #    flash('Text contains illegal character')
    #    LOG.debug('Text contains illegal character')
    #    return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
    LOG.debug(cmd)
    subprocess.run(cmd, shell=True)



    return send_file(os.path.join(app.config['UPLOAD_FOLDER'], destination))

@server_api.route('/addaudiotovideo', methods=['POST'])
def add_audio_to_video_then_respond():
    global globbal_id
    LOG.debug(f'{globbal_id} - Request Received - /addaudiotovideo')
    globbal_id+=int(1)
    
    if 'file' not in request.files:
        flash('No file part')
        LOG.debug('Request contained no file.')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}

    counter=int(0)
    audiotrimcommand = ""
    videoaddeffectcommand = ""

    for x in request.files.getlist('file'):
        if x.filename.strip().replace('"', '') == "command.txt":
            if counter==2:
                command = x.read().decode("utf-8")
                audiotrimcommand = command.split("\n")[0]
                videoaddeffectcommand = command.split("\n")[1]
        else:
            if counter==0:
                videothatwillbetrimmed = x
                if videothatwillbetrimmed == '':
                    flash('No selected file')
                    LOG.debug('Request filename empty.')
                    return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
            if counter==1:
                audiothatwillbetrimmed = x
                if audiothatwillbetrimmed == '':
                    flash('No selected file')
                    LOG.debug('Request filename empty.')
                    return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
        counter+=int(1)

    videofilePath = ''.join(random.sample(string.ascii_lowercase, 7)) + videothatwillbetrimmed.filename.strip().replace('"', '')
    audiofilePath = ''.join(random.sample(string.ascii_lowercase, 7)) + audiothatwillbetrimmed.filename.strip().replace('"', '')
    LOG.debug(f'Saving file: {videofilePath}')
    videothatwillbetrimmed.save(os.path.join(app.config['UPLOAD_FOLDER'], videofilePath))
    LOG.debug(f'Saving file: {audiofilePath}')
    audiothatwillbetrimmed.save(os.path.join(app.config['UPLOAD_FOLDER'], audiofilePath))

    destination = f"{globbal_id}_audiotovideo_output.mp4"

    cmd2=(f'ffmpeg -y -i "{videofilePath}" {videoaddeffectcommand} {audiotrimcommand} -i "{audiofilePath}" '+
         f' -filter_complex amix -map 0:v -map 0:a -map 1:a -c:v copy -c:a aac -strict experimental -async 1 "{destination}"')
    if ";" in cmd2:
        flash('Text contains illegal character')
        LOG.debug('Text contains illegal character')
        return json.dumps({'success':False}), 204, {'ContentType':'application/json'}
    LOG.debug(cmd2)
    subprocess.run(cmd2, shell=True)



    return send_file(os.path.join(app.config['UPLOAD_FOLDER'], destination))
