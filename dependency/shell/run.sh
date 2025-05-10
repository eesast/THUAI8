#!/usr/local
# === THUAI8 非对称竞技启动脚本 ===

# 运行目录与路径设置
python_dir=/usr/local/PlayerCode/CAPI/python/PyAPI
python_main_dir=/usr/local/PlayerCode/CAPI/python
playback_dir=/usr/local/output
map_dir=/usr/local/map
mkdir -p $playback_dir

# 模式初始化
if [[ "${MODE}" == "ARENA" ]]; then
    MODE_NUM=2
elif [[ "${MODE}" == "COMPETITION" ]]; then
    MODE_NUM=1
fi

# 默认参数赋值
: "${TEAM_SEQ_ID:=0}"
: "${TEAM_LABELS:=Buddhist:Monster}" 
: "${TEAM_LABEL:=Buddhist}"
: "${EXPOSED=1}"
: "${MODE_NUM=2}"
: "${GAME_TIME=600}"
: "${CONNECT_IP=172.17.0.1}"

# get_current_team_label() {
#     if [ $TEAM_SEQ_ID -eq $2 ]; then
#         echo "find current team label: $1"
#         current_team_label=$1
#     fi
# }

# read_array() {
#     callback=$1
#     echo "read array: set callback command: $callback"
    
#     IFS=':' read -r -a fields <<< "$2"

#     count=0 # loop count

#     for field in "${fields[@]}"
#     do
#         echo "parse field: $field"
#         param0=$field
        
#         # call command
#         run_command="$callback $param0 $count"
#         echo "Call Command: $run_command"
#         $run_command

#         count=$((count+1))
#     done
# }

function retry_command {
    local command="$1"
    local max_attempts=5
    local attempt_num=1
    local sleep_seconds=10

    while [ $attempt_num -le $max_attempts ]; do
        echo "Attempt $attempt_num / $max_attempts to run command: $command"

        eval $command &
        local PID=$!

        sleep $sleep_seconds

        if kill -0 $PID 2>/dev/null; then
            echo "Failed to connect to server. Retrying..."
            ((attempt_num++))
        else
            echo "Connected to server successfully."
            return 0
        fi
    done

    echo "Failed to connect to server after $max_attempts attempts."
    return 1
}

if [ "$TERMINAL" = "SERVER" ]; then
    map_path=$map_dir/$MAP_ID.txt
    echo "Starting THUAI8 SERVER..."

    if [ $EXPOSED -eq 1 ]; then
        nice -10 ./Server --port 8888 --teamCount 2 --CharacterNum 6 --resultFileName $playback_dir/result --gameTimeInSecond $GAME_TIME --mode $MODE_NUM --mapResource $map_path --url $SCORE_URL --token $TOKEN --fileName $playback_dir/playback --startLockFile $playback_dir/start.lock > $playback_dir/server.log 2>&1 &
    else
        nice -10 ./Server --port 8888 --teamCount 2 --CharacterNum 6 --resultFileName $playback_dir/result --gameTimeInSecond $GAME_TIME --mode $MODE_NUM --mapResource $map_path --notAllowSpectator --url $SCORE_URL --token $TOKEN --fileName $playback_dir/playback --startLockFile $playback_dir/start.lock > $playback_dir/server.log 2>&1 &
    fi

    server_pid=$!
    echo "Server PID: $server_pid"
    ls $playback_dir

    echo "SCORE URL: $SCORE_URL"
    echo "FINISH URL: $FINISH_URL"

    echo "waiting..."
    sleep 60
    echo "watching..."
    
    if [ ! -f $playback_dir/start.lock ]; then
        echo "Failed to start game."
        touch temp.lock
        mv -f temp.lock $playback_dir/playback.thuaipb
        kill -9 $server_pid
        finish_payload='{"status": "Crashed", "scores": [0, 0]}'
        curl $FINISH_URL -X POST -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" -d "${finish_payload}" > $playback_dir/send.log 2>&1
    else
        echo "Game is started."
        ps -p $server_pid
        while [ $? -eq 0 ]
        do
            sleep 1
            ps -p $server_pid > /dev/null 2>&1
        done
        echo "Server is down."
    fi

elif [ "$TERMINAL" = "CLIENT" ]; then
    echo "Starting CLIENT for team $TEAM_LABEL (ID: $TEAM_SEQ_ID)"
    
    if [[ "$TEAM_LABEL" == "Buddhist" ]]; then
        players=( "Buddhist1" "Buddhist2" "Buddhist3" "Buddhist4" "Buddhist5" "Buddhist6" )
    elif [[ "$TEAM_LABEL" == "Monster" ]]; then
        players=( "Monster1" "Monster2" "Monster3" "Monster4" "Monster5" "Monster6" )
    else
        echo "Error: Invalid Team Label $TEAM_LABEL"
        exit 1
    fi
    
    pushd /usr/local/code 
        for idx in "${!players[@]}"; do
            code_name="${players[$idx]}"
            if [[ "$TEAM_LABEL" == "Buddhist" ]]; then
                s_param=0
            else
                s_param=1
            fi
            if [[ -f "./$code_name.py" ]]; then
                echo "Found ./$code_name.py"
                cp -r "$python_main_dir" "${python_main_dir}${idx}"
                cp -f "./$code_name.py" "${python_main_dir}${idx}/PyAPI/AI.py"
                command="nice -n 0 python3 ${python_main_dir}${idx}/PyAPI/main.py -I $CONNECT_IP -P $PORT -t $TEAM_SEQ_ID -s $s_param -p $idx > $playback_dir/team$TEAM_SEQ_ID-$code_name.log 2>&1 &"
                retry_command "$command" > $playback_dir/client$TEAM_SEQ_ID-$idx.log &
            elif [[ -f "./$code_name" ]]; then
                echo "find ./$code_name"
                command="nice -n 0 ./$code_name -I $CONNECT_IP -P $PORT -t $TEAM_SEQ_ID -s $s_param -p $idx > $playback_dir/team$TEAM_SEQ_ID-$code_name.log 2>&1 &"
                retry_command "$command" > $playback_dir/client$TEAM_SEQ_ID-$idx.log &
            else
                echo "ERROR: $code_name not found."; continue
            fi
        done
        sleep $((GAME_TIME+90))
    popd 

else
    echo "ERROR: TERMINAL must be SERVER or CLIENT."; exit 1
fi
