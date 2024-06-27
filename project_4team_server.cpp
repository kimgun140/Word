#include <mariadb/conncpp.hpp>
#include <cstdlib>
#include <iostream>
#include <fstream>
#include <unistd.h>
#include <string>
#include <cstring>
#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <pthread.h>
#include <curl/curl.h>
#include <sstream>
#include <iomanip>
#include "json.hpp"
#define IMAGE_BUFFER_SIZE 1024
#define BUF_SIZE 1000
#define MAX_CLNT 256
#define NAME_SIZE 20
using json = nlohmann::json;

size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp);
void * backgroundthread(void * arg);
void error_handling(const char * msg);
void login_logic(int clnt_sock);
void Parsing(std::string readBuffer,std::string Do);

int clnt_cnt = 0; // 소켓 디스크립터의 인덱스 번호 역할
int clnt_socks[MAX_CLNT]; // 소켓 디스크립터 담을 256개
std::vector<int> chat_user_socks;
std::vector<int> chat_counselor_socks; 
pthread_mutex_t mutx; // 뮤텍스 변수


class DB{
public:
    sql::Connection* conn;
    DB(){}
    void connect(){
        try{
            sql::Driver* driver= sql::mariadb::get_driver_instance();
            sql::SQLString url=("jdbc:mariadb://10.10.21.118:3306/word");        
            sql::Properties properties({ {"user", "4TEAM"}, {"password", "1234"} }); 
            conn = driver->connect(url, properties);
        }
        catch(sql::SQLException& e){
            std::cerr << "Error Connecting to MariaDB Platform: " << e.what() << std::endl;
        }
    }
 
    ~DB(){delete conn;}
};
//////////////////
std::string read(int clnt_sock, DB& data);
void API(int clnt_sock, std::string want_word,std::string Do,DB& database);
class Login{
    DB& DB_login;
public:    
    Login(DB& DB_init): DB_login(DB_init){}
    
    int user_Login_logic(int clnt_sock,std::vector<std::string> login_pw){
        std::string input_ID = login_pw[0];
        std::string output_ID ;
        std::string input_PW = login_pw[1];
        std::string output_PW;
        while(1){
            std::cout<<"로그인로직\n";
            std::cout<<"받아온아이디 = "<<login_pw[0]<<std::endl;
            std::cout<<"받아온비번 = "<<login_pw[1]<<std::endl;
            try{
                std::cout<<"로그인 트라이 진입"<<std::endl;
                std::unique_ptr <sql::Statement > con(DB_login.conn -> createStatement()); //객체 생성
                sql::ResultSet *res =con->executeQuery("SELECT id, pw FROM user WHERE id = '" + input_ID + "'AND pw = '" + input_PW + "'");
                while (res->next())
                {
                    output_ID = res->getString(1);
                    output_PW = res->getString(2);
                }
                if (input_ID == output_ID && input_PW == output_PW)
                {                                                       // 로그인성공 조건통과
                    update_discripter(output_ID, clnt_sock,0);          // 디스크립터 업데이트 함수
                    write(clnt_sock, "로그인 되었습니다", strlen("로그인 되었습니다")); // 로그인 성공 메세지 전달
                    std::cout << "success =" << output_ID << std::endl;
                    std::cout << "success =" << output_PW << std::endl;
                    return 0;
                }
                else
                {
                    write(clnt_sock, "일치하는 정보가 없습니다", strlen("일치하는 정보가 없습니다")); // 로그인 실패 메세지 전달
                    std::cout << "fail =" << output_ID << std::endl;
                    std::cout << "fail =" << output_PW << std::endl;
                    return -1;
                }
            }
            catch(sql::SQLException& e){
                std::cerr << "Error Connecting to MariaDB Platform: " << e.what() << std::endl;
                return -1;
            }
        }
    }
    int counselor_Login_logic(int clnt_sock,std::vector<std::string> login_pw)
    {
        std::string input_ID = login_pw[0];
        std::string output_ID ;
        std::string input_PW = login_pw[1];
        std::string output_PW;
        while(1){
            std::cout<<"로그인로직\n";
            std::cout<<"받아온아이디 = "<<login_pw[0]<<std::endl;
            std::cout<<"받아온비번 = "<<login_pw[1]<<std::endl;
            try
            {
                std::unique_ptr<sql::Statement> con(DB_login.conn->createStatement()); // 객체 생성
                sql::ResultSet *res = con->executeQuery("SELECT id, pw FROM counselor WHERE id = '" + input_ID + "' AND pw = '" + input_PW + "'");
                while (res->next())
                {
                    output_ID = res->getString(1);
                    output_PW = res->getString(2);
                }
                if (input_ID == output_ID && input_PW == output_PW)
                {                                                       // 로그인성공 조건통과
                    update_discripter(output_ID, clnt_sock,1);          // 디스크립터 업데이트 함수
                    write(clnt_sock, "로그인 되었습니다", strlen("로그인 되었습니다")); // 로그인 성공 메세지 전달
                    std::cout << "success =" << output_ID << std::endl;
                    std::cout << "success =" << output_PW << std::endl;
                    return 0;
                }
                else
                {
                    write(clnt_sock, "일치하는 정보가 없습니다", strlen("일치하는 정보가 없습니다")); // 로그인 실패 메세지 전달
                    std::cout << "fail =" << output_ID << std::endl;
                    std::cout << "fail =" << output_PW << std::endl;
                    return -1;
                }
            }
            catch (sql::SQLException &e)
            {
                std::cerr << "Error Connecting to MariaDB Platform: " << e.what() << std::endl;
                return -1;
            }
        }
    }
    void update_discripter(std::string ID, int clnt_sock, int user_or_counselor){
        if(user_or_counselor ==0){
            try {
                std::unique_ptr<sql::PreparedStatement> insert_info(DB_login.conn->prepareStatement("UPDATE user SET DISCRIPTER = ? WHERE id = ?"));
                insert_info->setInt(1, clnt_sock);//디스크립터
                insert_info->setString(2, ID);//아이디
                insert_info->executeQuery();
            }
            catch(sql::SQLException& e){
                std::cerr << "Error cc 디스크립터 업데이트 new task: " << e.what() << std::endl;
            }
        }
        else{
            try {
                std::unique_ptr<sql::PreparedStatement> insert_info(DB_login.conn->prepareStatement("UPDATE counselor SET DISCRIPTER = ? WHERE id = ?"));
                insert_info->setInt(1, clnt_sock);//디스크립터
                insert_info->setString(2, ID);//아이디
                insert_info->executeQuery();
            }
            catch(sql::SQLException& e){
                std::cerr << "Error cs 디스크립터 업데이트 new task: " << e.what() << std::endl;
            }
        }
    }
};
/////////////////
class Signup{
    DB &DB_signup;
public:
    Signup(DB &DB_init) : DB_signup(DB_init) {}

    void user_Signup_logic(int clnt_sock, std::vector<std::string> signup_pw_msg)
    {
        int already_count = -1;
        std::cout<<"등록할 아이디"<<signup_pw_msg[0]<<std::endl;
        std::cout<<"등록할 비번"<<signup_pw_msg[1]<<std::endl;

        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_signup.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT COUNT(*) FROM user WHERE id = '" + signup_pw_msg[0] + "'");
            while (res->next())
            {
                already_count = res->getInt(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error user 중복아이디 찾기 new task: " << e.what() << std::endl;
            exit(1);
        }
        if(already_count >=1)
        {
            write(clnt_sock,"중복된아이디",strlen("중복된아이디"));
        }

        else
        {
            try
            {
                std::unique_ptr<sql::Statement> stmnt(DB_signup.conn->createStatement());
                sql::ResultSet *res = stmnt->executeQuery("INSERT user VALUES (default, '"+ signup_pw_msg[0] + "', '"+ signup_pw_msg[1] +"'," + std::__cxx11::to_string(clnt_sock) + ")");
            }
            catch (sql::SQLException &e)
            {
                std::cerr << "Error user 회원등록 인서트 new task: " << e.what() << std::endl;
                exit(1);
            }
            write(clnt_sock,"회원가입성공",strlen("회원가입성공"));
        }
    }
    void counselor_Signup_logic(int clnt_sock, std::vector<std::string> signup_pw_msg)
    {
        std::cout<<"등록할 아이디"<<signup_pw_msg[0]<<std::endl;
        std::cout<<"등록할 비번"<<signup_pw_msg[1]<<std::endl;
        int already_count = -1;
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_signup.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT COUNT(*) FROM counselor WHERE id = '" + signup_pw_msg[0] + "'");
            while (res->next())
            {
                already_count = res->getInt(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error counselor 중복아이디 찾기 new task: " << e.what() << std::endl;
            exit(1);
        }

        if(already_count >=1)
        {
            std::cout<<"중복된아이디 전송\n";
            write(clnt_sock,"중복된아이디",strlen("중복된아이디"));
        }
        else
        {
            try
            {
                 std::cout<<"중복된아이디 없어서 인서트문 시작\n";
                std::unique_ptr<sql::Statement> stmnt(DB_signup.conn->createStatement());
                sql::ResultSet *res = stmnt->executeQuery("INSERT counselor VALUES (default, '"+ signup_pw_msg[0] + "', '"+ signup_pw_msg[1] +"'," + std::__cxx11::to_string(clnt_sock) + ")");
            }
            catch (sql::SQLException &e)
            {
                std::cerr << "Error counselor 회원등록 인서트 new task: " << e.what() << std::endl;
                exit(1);
            }
            write(clnt_sock,"회원가입성공",strlen("회원가입성공"));
        }
    }
};
/////////////////
class User_func{
    DB& DB_user_func;
    
    struct correct_flag{
        std::string user_id;
        std::string WORD;
        std::string DEFINITION;
        int OX;
    }correct_struct;

public:
    User_func(DB& DB_init) : DB_user_func(DB_init) {}
    void choice_menu(int clnt_sock){
        std::cout<<"고객 초이스함수진입\n";
        std::string compare_msg;
        bool end_clnt;
        while(1){
            compare_msg =read(clnt_sock,DB_user_func);
            std::cout<<compare_msg<<std::endl;
            if(compare_msg == "학습하기")
            {
                study_word(clnt_sock);
            }
            else if(compare_msg == "문제풀기")
            {
                test_word(clnt_sock); //안푼 문제 전송
                find_user_id(clnt_sock); //유저 아이디 알아내기
                while(1)
                {
                    compare_msg =read(clnt_sock,DB_user_func);
                    std::cout<<"답안 = "<<compare_msg <<std::endl;
                    if(compare_msg == "답안제출")
                    {
                        break;
                    }
                    else if(compare_msg == "연결종료")
                    {
                        end_clnt = true;
                        break;
                    }
                    else
                    {   
                        grade_score(clnt_sock,compare_msg); //유저가 입력한 WORD,DEFINITION에 대한 OX 알아내기
                        insert_history(clnt_sock,compare_msg,correct_struct); // quiz_history TABLE에 insert하기
                        insert_socre(correct_struct.user_id); //문제 다 풀었는지 검사하고 다 풀었으면 score TABLE에 INSERT 하기
                    }
                }
            }
            else if(compare_msg == "상담")
            {
                user_chat_wait(clnt_sock);
            }
            else if(compare_msg == "문제풀이불러오기")
            {
                find_user_id(clnt_sock); //유저 아이디 알아내기
                select_history(clnt_sock);
            }
            else if(compare_msg == "연결종료")
            {
                break;
            }
            if(end_clnt == true)
            {
                break;
            }
        }
    }

    void study_word(int clnt_sock){
        
        std::string jsonstring;
        json json_k_v;
        std::vector<std::string> word_vector;
        std::vector<std::string> definition_vector;
        word_vector.clear();
        definition_vector.clear();
        try
        {
            std::cout << "study_word try진입" << std::endl;
            std::unique_ptr<sql::Statement> con(DB_user_func.conn->createStatement()); 
            sql::ResultSet *res = con->executeQuery("SELECT * FROM quiz");
            while (res->next())
            {
                word_vector.emplace_back(res->getString(1));
                definition_vector.emplace_back(res->getString(2));
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error Connecting to MariaDB Platform: " << e.what() << std::endl;
        }

        for(int i =0 ; i < word_vector.end() - word_vector.begin() ; i++)
        {
            json_k_v[word_vector[i]] = definition_vector[i];
            jsonstring = json_k_v.dump();
            usleep(1000000);
            write(clnt_sock,jsonstring.c_str(),strlen(jsonstring.c_str()));

        }
        word_vector.clear();
        definition_vector.clear();
        usleep(1000000);
        write(clnt_sock,"데이터전송종료",strlen("데이터전송종료")); //끝을알리는 데이터 전송
        
    }

    void test_word(int clnt_sock){

        int flag = 0;
        std::string id;
        std::string jsonstring;
        json json_k_v;
        std::vector<std::string> write_word;
        std::vector<std::string> write_definition;
        // std::vector<std::string> history_definition;
        write_word.clear();
        write_definition.clear();
        // history_definition.clear();

        try
        {
            std::cout << "test_word 아이디찾기 try진입" << std::endl;
            std::unique_ptr<sql::Statement> con(DB_user_func.conn->createStatement()); 
            sql::ResultSet *res = con->executeQuery("SELECT id FROM user WHERE DISCRIPTER = " + std::__cxx11::to_string(clnt_sock));
            while (res->next())
            {
                correct_struct.user_id = res->getString(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 디스크립터로 아이디찾기: " << e.what() << std::endl;
        }


        try
        {
            std::cout << "test_word quiz단어 가지고오기 try진입" << std::endl;
            std::unique_ptr<sql::Statement> con(DB_user_func.conn->createStatement()); 
            sql::ResultSet *res = con->executeQuery("SELECT * FROM quiz WHERE DEFINITION NOT IN (SELECT DEFINITION FROM quiz_history WHERE user_id ='" + correct_struct.user_id +"')");
            while (res->next())
            {
                write_word.emplace_back(res->getString(1));
                write_definition.emplace_back(res->getString(2));
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error test_word select * from quiz: " << e.what() << std::endl;
        }

        for(int i = 0 ; i < write_word.end() - write_word.begin() ; i++)
        {
            if(write_word[i].empty())
            {
                write(clnt_sock,"{\"already\":\"solve\"}",strlen("{\"already\":\"solve\"}"));
                break;
            }
            else{
                std::cout<<"write_word["<<i<<"] = "<<write_word[i]<<std::endl;
                std::cout<<"quiz_definition["<<i<<"] = "<<write_definition[i]<<std::endl;
                json_k_v.clear();
                json_k_v[write_word[i]] = write_definition[i];
                jsonstring = json_k_v.dump();
                sleep(1.5);
                write(clnt_sock,jsonstring.c_str(),strlen(jsonstring.c_str()));
            }
        }
        sleep(1.5);
        write(clnt_sock,"데이터전송종료",strlen("데이터전송종료")); //끝을알리는 데이터 전송
        
    }

    void user_chat_wait(int clnt_sock){ // 고객이 대기
        int user_counselor[2];
        char msg[BUF_SIZE];
        char end_msg[BUF_SIZE] = "연결이 종료되었습니다.\n";
        std::cout<<"user_chat_wait 입장"<<std::endl;

        chat_user_socks.emplace_back(clnt_sock);

        while(1){
            if(chat_user_socks.size() >= 1 && chat_counselor_socks.size() >= 1){
                write(clnt_sock,"채팅가능",strlen("채팅가능"));
                user_counselor[0] = chat_user_socks[0];
                user_counselor[1] = chat_counselor_socks[0];
                break;
            }
        }

        std::cout<<"유저 채팅가능상태\n";

        while(1){ //대화하는 와일문
            memset(msg,0,sizeof(msg));
            if(read(clnt_sock,msg,sizeof(msg)) > 0){
                std::cout<<msg<<std::endl;
                for(int i = 0 ; i< 2 ; i++){
                    write(user_counselor[i],msg,strlen(msg));
                }
            }
            else{
                for(int i = 0 ; i < 2 ; i++)
                    write(user_counselor[i],end_msg,strlen(end_msg));
                break;
            }
        }
    }
   
    void find_user_id(int clnt_sock){
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT id FROM user WHERE DISCRIPTER = " + std::__cxx11::to_string(clnt_sock));
            while(res->next())
            {
                correct_struct.user_id = res->getString(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 정답맞추기 quiz select new task: " << e.what() << std::endl;
            exit(1);
        }
    }

    void grade_score(int clnt_sock,std::string json_answers){
        std::cout<<"grade_score함수진입"<<std::endl;
        json json_k_v;
        std::string j_key;
        std::string j_value;
        std::string out_word;
        int correct_count=0;
        char* buf = nullptr;
        int len = 0;

        json_k_v.clear();
        json_k_v = json::parse(json_answers);
            
        for (auto& item : json_k_v.items()) 
        {
            std::cout << "Key: " << item.key() << std::endl;
            std::cout << "valuse: "<<item.value() <<std::endl;
            correct_struct.WORD = item.key();
            correct_struct.DEFINITION = item.value();
        }
        
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT WORD FROM quiz WHERE DEFINITION = '" + correct_struct.DEFINITION + "'");
            while(res->next())
            {
                out_word = res->getString(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 정답맞추기 quiz select new task: " << e.what() << std::endl;
            exit(1);
        }

        if(correct_struct.WORD == out_word)
        {   
            correct_struct.OX = 1;

            if (read_image_from_file("/home/lms/다운로드/correct.png", &buf, &len))
            {
                // 이미지 데이터 크기 전송
                int image_size = htonl(len); // 크기를 네트워크 바이트 순서로 변환
                if (send(clnt_sock, &image_size, sizeof(image_size), 0) == -1)
                {
                    perror("이미지 크기 전송 실패");
                }
                std::cout << "이미지 크기" << image_size << std::endl;

                // 이미지 데이터 전송
                if (send(clnt_sock, buf, len, 0) == -1)
                {
                    perror("이미지 전송 실패");
                }
                else
                {
                    std::cout << "정답 이미지 전송 완료" << std::endl;
                }
                free(buf);
            }

        }

        else
        {
            correct_struct.OX = 0;
            if (read_image_from_file("/home/lms/다운로드/not_correct.jpg", &buf, &len))
            {
                // 이미지 데이터 크기 전송
                int image_size = htonl(len); // 크기를 네트워크 바이트 순서로 변환
                if (send(clnt_sock, &image_size, sizeof(image_size), 0) == -1)
                {
                    perror("이미지 크기 전송 실패");
                }
                std::cout << "이미지 크기" << image_size << std::endl;

                // 이미지 데이터 전송
                if (send(clnt_sock, buf, len, 0) == -1)
                {
                    perror("이미지 전송 실패");
                }
                else
                {
                    std::cout << "오답 이미지 전송 완료" << std::endl;
                }
                free(buf);
            }
        }
    }
 
    void insert_history(int clnt_sock,std::string compare_msg,correct_flag& correct_struct){
        
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("INSERT INTO quiz_history VALUES( '" + correct_struct.user_id + "', '"
            + correct_struct.WORD + "', '" + correct_struct.DEFINITION + "', " + std::__cxx11::to_string(correct_struct.OX) + ", default)");

        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 정답맞추기 quiz select new task: " << e.what() << std::endl;
            exit(1);
        }
    }

    void insert_socre(std::string user_id){
        int count_history;
        int count_quiz;
        int count_O;
        std::cout<<"인서트스코어함수진입\n";
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT COUNT(*) FROM quiz_history WHERE user_id = '"+ correct_struct.user_id +"'");
            while(res->next())
            {
                count_history = res->getInt(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 카운트 quiz_history new task: " << e.what() << std::endl;
            exit(1);
        }
        std::cout<<"count_history = "<<count_history<<std::endl;
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT COUNT(*) FROM quiz");
            while(res->next())
            {
                count_quiz = res->getInt(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 카운트 quiz select new task: " << e.what() << std::endl;
            exit(1);
        }

        std::cout<<"count_quiz = "<<count_quiz<<std::endl;
        if(count_history == count_quiz)
        {
            try
            {
                std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
                sql::ResultSet *res = stmnt->executeQuery("SELECT COUNT(*) FROM quiz_history WHERE OX = 1");
                while(res->next())
                {
                    count_O = res->getInt(1);
                }
            }
            catch (sql::SQLException &e)
            {
                std::cerr << "Error 카운트 quiz_history count new task: " << e.what() << std::endl;
                exit(1);
            }
                
            std::cout<<"count_O = "<<count_O<<std::endl;
            try
            {
                std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
                sql::ResultSet *res = stmnt->executeQuery("INSERT INTO USER_SCORE VALUES('" + correct_struct.user_id +"', " + std::__cxx11::to_string(round((count_O / count_quiz) *100)) + ")");
            }
            catch (sql::SQLException &e)
            {
                std::cerr << "Error 점수 insert new task: " << e.what() << std::endl;
                exit(1);
            }

        }
    }

    void select_history(int clnt_sock)
    {
        std::cout<<"셀렉트히스토리함수진입\n";
        std::string user_id;
        std::vector<std::string> USER_WORD;
        std::vector<std::string> DEFINITION;
        std::vector<std::string> OX;
        std::vector<std::string> quiz_date;
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_user_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT * FROM quiz_history WHERE user_id = '"+ correct_struct.user_id +"'");
            while(res->next())
            {
                user_id = res->getString(1);
                USER_WORD.emplace_back(res->getString(2));
                DEFINITION.emplace_back(res->getString(3));
                OX.emplace_back(std::__cxx11::to_string(res->getInt(4)));
                quiz_date.emplace_back(res->getString(5));
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 불러오기 quiz_history new task: " << e.what() << std::endl;
            exit(1);
        }
        for(int i = 0 ; i < USER_WORD.end() - USER_WORD.begin() ; i++)
        {
            sleep(1.5);
            write(clnt_sock,USER_WORD[i].c_str(),strlen(USER_WORD[i].c_str())); //고객이 쓴 답
            sleep(1.5);
            write(clnt_sock,DEFINITION[i].c_str(),strlen(DEFINITION[i].c_str()));//뜻
            sleep(1.5);
            write(clnt_sock,OX[i].c_str(),strlen(OX[i].c_str()));//정답여부
            sleep(1.5);
            write(clnt_sock,quiz_date[i].c_str(),strlen(quiz_date[i].c_str()));//푼 날짜
        }
        sleep(1.5);
        write(clnt_sock,"전송완료",strlen("전송완료"));
    }

    bool read_image_from_file(const char *filepath, char **buf, int *len)
    // 파일의 경로를 입력받아 파일을 읽고 내용을 메모리에 저장하는 함수.
    // const char *filepath : 읽을 파일의 경로
    // char **buf : 파일 내용을 저장할 버퍼의 주소.
    // int *len : 파일 크기를 저장할 변수의 주소를 가르키는 포인트
    {
    FILE *fp = fopen(filepath, "rb");
    // fopen 파일 여는 함수
    // filepath 열 파일의 경로
    // "rb" : 파일을 읽기 모드로 열고 이진 모드로 읽겠다는 의미
    // rb는 r 모드와 b 모드의 조합입니다.
    // r 모드 : read를 의미하며 파일 읽기 전용으로 열기.
    // b 모드 : binary를 의미. 이 모드는 파일을 바이너리 모드로 연다.
    // 바이너리 모드는 파일을 있는 그대로 읽고 쓰는 모드. 텍스트 모드에서 줄바꿈이나 다른 내용 해석이 바이러니에서는 발생하지 않음.
    // 즉 rb는 파일을 바이너리 모드에서 읽기 전용으로 열겠다는 의미이며 이미지, 오디오, 비디오, 실행 파일등의 바이너리 데이터를 읽을 때, 파일의 정확한 바이트 단위 데이터를 유지하고 싶을때 사용합니다.

    if (!fp)
    {
        perror("파일 열기 실패");
        return false;
    }

    fseek(fp, 0, SEEK_END);
    // fseek 함수는 파일 포인터의 위치를 변경하는 함수
    // fp는 파일 포인터
    // 0 은 오프셋 값으로 이동할 바이트 수 입니다.. 파일의 기준 위치로부터 오프셋을 나타내는 정수. 이 오프셋은 양수 또는 음수가 될 수 있다. 0을 지정하여 파일 끝에서의 위치를 이동하지 않음을 뜻함.
    // SEEK_END: 파일 끝을 기준으로한다, 즉 파일 포인터를 파일 끝으로 이동.
    // SEEK_SET: 파일의 시작을 기준으로 오프셋을 설정합니다.
    // SEEK_CUR: 현재 파일 포인터의 위치를 기준으로 오프셋을 설정합니다.

    *len = ftell(fp);
    // ftell 함수는 현재 파일 포인터의 위치를 반환.
    // 파일 끝에서의 위치는 파일의 크기와 같다.
    // *len에 파일의 크기 저장.

    fseek(fp, 0, SEEK_SET);
    // 파일 포인터를 다시 파일 시작 위치로 이동. 이후의 파일 읽기 또는 쓰기 작업이 파일의 처음부터 시작되기 위함.
    // 파일 포인터를 처음으로 되돌리지 않으면 이후 읽기 작업은 파일의 끝 부분에서 시작하기에 문제를 발생시킬 수 있습니다.

    *buf = (char *)malloc(*len);
    // len의 크기만큼 메모리 할당
    // 할당된 메모리의 시작 주소를 buf에 저장.

    if (!*buf)
    {
        perror("메모리 할당 실패");
        fclose(fp);
        return false;
    }

    size_t bytesRead = fread(*buf, 1, *len, fp);
    // fread는 파일에서 데이터를 읽는 함수.
    // buf는 데이터를 저장할 버퍼
    // 1은 읽을 데이터의 단위 크기(바이트 단위)
    // len 읽을 데이터의 총 크기
    // fp 파일 포인터
    // bytesRead에 실제로 읽은 바이트 수를 저장.

    if (bytesRead != *len)
    {
        perror("파일 읽기 실패");
        free(*buf);
        fclose(fp);
        return false;
    }

    fclose(fp);
    // 파일 닫기
    return true;
}
   
};
/////////////////
class Counselor_func{
    DB& DB_Counselor_func;
public:
    Counselor_func(DB& DB_init) : DB_Counselor_func(DB_init) {}

    void choice_menu(int clnt_sock){
        std::cout<<"상담원 초이스함수진입\n";
        std::string compare_msg;
        bool end_clnt;
        while(1){
            compare_msg =read(clnt_sock,DB_Counselor_func);
            std::cout<<"상담원이 고른 메뉴 : '"<<compare_msg<<"'"<<std::endl;
            if(compare_msg =="개인")
            {
                inquiry_personal_grade(clnt_sock);
            }
            else if(compare_msg == "그래프보기")
            {
                inquiry_all_grade(clnt_sock);
            }
            else if(compare_msg == "문제관리")
            {
                while(1){
                    compare_msg = read(clnt_sock,DB_Counselor_func);
                    std::cout<<compare_msg<<std::endl;
                    if(compare_msg == "미리보기")
                    {
                        preview_word(clnt_sock);
                    }
                    else if(compare_msg == "추가")
                    {
                        add_question(clnt_sock);
                    }
                    else if(compare_msg == "뒤로가기")
                    {
                        break;
                    }
                    else if(compare_msg == "연결종료")
                    {
                        end_clnt = true;
                        break;
                    }
                    else
                    {
                        std::cout<<"이상한 메시지 = "<<compare_msg<<std::endl;
                    }
                }

            }
            else if(compare_msg == "상담")
            {
                counselor_chat_wait(clnt_sock);
            }
            // else if(compare_msg =="뒤로가기")
            // {
            //     break;
            // }
            else if(compare_msg == "연결종료")
            {
                break;
            }
            if(end_clnt ==true)
            {
                break;
            }
        }
    }

    void inquiry_personal_grade(int clnt_sock)
    {   
        std::cout<<"개인 함수 진입\n";
        std::string input_id;
        std::vector<std::string> all_id;
        int SCORE=0;

        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_Counselor_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT ID FROM user");
            while(res->next())
            {
                all_id.emplace_back(res->getString(1));
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 모든 유저아이디 검색 new task: " << e.what() << std::endl;
            exit(1);
        }

        for(int i = 0 ; i < all_id.end() - all_id.begin() ; i ++)
        {
            std::cout<<"전송하는 아이디 : "<<all_id[i]<<std::endl;
            
            write(clnt_sock, all_id[i].c_str(),strlen(all_id[i].c_str())); //모든 유저의 아이디 전송
            usleep(100000);
        }
        usleep(100000);
        std::cout<<"전송완료\n";
        write(clnt_sock,"전송완료",strlen("전송완료"));

        input_id = read(clnt_sock, DB_Counselor_func);
        std::cout<<"요청온 점수 볼 아이디 '"<<input_id<<"'\n";
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_Counselor_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT SCORE FROM USER_SCORE WHERE user_id = '" + input_id + "'");
            while(res->next())
            {
                SCORE = res->getInt(1);
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 점수 검색 new task: " << e.what() << std::endl;
            exit(1);
        }
        std::cout<<"전송하는 점수 : "<<SCORE<<std::endl;
        write(clnt_sock,std::__cxx11::to_string(SCORE).c_str(),strlen(std::__cxx11::to_string(SCORE).c_str()));
        
    }

    void inquiry_all_grade(int clnt_sock)
    {   
        std::cout<<"그래프보기 함수진입\n";
        std::vector<std::string> user_id;
        std::vector<std::string> SCORE;
        try
        {
            std::unique_ptr<sql::Statement> stmnt(DB_Counselor_func.conn->createStatement());
            sql::ResultSet *res = stmnt->executeQuery("SELECT * FROM USER_SCORE");
            while(res->next())
            {   
                user_id.emplace_back(res->getString(1));
                SCORE.emplace_back(std::__cxx11::to_string(res->getInt(2)));
            }
        }
        catch (sql::SQLException &e)
        {
            std::cerr << "Error 모든 유저아이디 검색 new task: " << e.what() << std::endl;
            exit(1);
        }

        json id_score;

        for(int i = 0 ; i < user_id.end() - user_id.begin() ; i++)
        {
            id_score[user_id[i]] = SCORE[i];
            std::string id_score_str = id_score.dump();
            std::cout<<id_score_str<<std::endl;
            write(clnt_sock,id_score_str.c_str(),strlen(id_score_str.c_str()));
            usleep(100000);
        }
        usleep(100000);
        write(clnt_sock,"전송완료",strlen("전송완료"));
    }

    void preview_word(int clnt_sock)
    {
        std::cout<<"프리뷰함수진입\n";
        std::string compare_msg;
        compare_msg = read(clnt_sock,DB_Counselor_func);
        std::cout<<"요청온 미리보기 단어 = "<<compare_msg<<std::endl;
        

        API(clnt_sock, compare_msg,"미리보기",DB_Counselor_func);

        // std::cout<<"결과 = "<<preview_json[0]<<std::endl;
        // write(clnt_sock,preview_json[0].c_str(),strlen(preview_json[0].c_str()));
    }

    void add_question(int clnt_sock)
    {
        std::string compare_msg;
        std::vector<std::string> word_vector;
        compare_msg = read(clnt_sock,DB_Counselor_func);
        std::cout<<compare_msg<<std::endl;
        API(clnt_sock, compare_msg,"추가",DB_Counselor_func);

    }

    void counselor_chat_wait(int clnt_sock){ // 상담사가 대기
        int user_counselor[2];
        char msg[BUF_SIZE];
        char end_msg[BUF_SIZE] = "연결이 종료되었습니다.\n";
        std::cout<<"counselor_chat_wait 입장"<<std::endl;

        chat_counselor_socks.emplace_back(clnt_sock);

        while(1){
            if(chat_user_socks.size() >= 1 && chat_counselor_socks.size() >= 1){
                write(clnt_sock,"채팅가능",strlen("채팅가능"));
                user_counselor[0] = chat_user_socks[0];
                user_counselor[1] = chat_counselor_socks[0];
                break;
            }
        }
        std::cout<<"상담사 채팅가능상태\n";

        while(1){ //대화하는 와일문
            memset(msg,0,sizeof(msg));
            if(read(clnt_sock,msg,sizeof(msg)) > 0){
                std::cout<<msg<<std::endl;
                for(int i = 0 ; i< 2 ; i++){
                    write(user_counselor[i],msg,strlen(msg));
                }
            }
            else{
                for(int i = 0 ; i < 2 ; i++)
                    write(user_counselor[i],end_msg,strlen(end_msg));
                break;
            }
        }
    }
};
/////////////////
int main(int argc , char *argv[])
{
    int serv_sock, clnt_sock;
    struct sockaddr_in serv_adr, clnt_adr;
    socklen_t clnt_adr_sz;
    pthread_t t_id;

    pthread_mutex_init(&mutx, NULL);
    serv_sock = socket(PF_INET, SOCK_STREAM, 0);

    if (argc != 2) {
        printf("Usage : %s <port>\n", argv[0]);
        exit(1);
    }
    memset(&serv_adr, 0, sizeof(serv_adr));
    serv_adr.sin_family = AF_INET;
    serv_adr.sin_addr.s_addr = htonl(INADDR_ANY);
    serv_adr.sin_port = htons(atoi(argv[1]));

    if (bind(serv_sock, (struct sockaddr *) &serv_adr, sizeof(serv_adr)) == -1)
        error_handling("bind() error");
    if (listen(serv_sock, 5) == -1)
        error_handling("listen() error");

    while (1) {
        try{
            clnt_adr_sz = sizeof(clnt_adr);
            clnt_sock = accept(serv_sock, (struct sockaddr *) &clnt_adr, &clnt_adr_sz);
            printf("Connected client IP: %s \n", inet_ntoa(clnt_adr.sin_addr));
            pthread_mutex_lock(&mutx);
            clnt_socks[clnt_cnt++] = clnt_sock;
            pthread_mutex_unlock(&mutx);
            pthread_create(&t_id, NULL, backgroundthread, (void *) &clnt_sock);
            pthread_detach(t_id);
        }
        catch(std::exception &e){
            std::cerr<<"error : "<< &e <<std::endl;
        }

    }
    close(serv_sock);
    return 0;
}

int login_assign(int clnt_sock,int identified_flag, DB& database){

    Login login_instance(database);
    Signup signup_instance(database);
    std::string compare_msg;
    std::string ID, PW;
    std::vector<std::string> login_pw_msg;
    std::vector<std::string> signup_pw_msg;
    int login_flag = -1;

    compare_msg=read(clnt_sock,database); //로그인인가 회원가입인가.
    std::cout<<compare_msg<<std::endl;
    if(compare_msg == "로그인요청\n" || compare_msg == "로그인요청"){ //로그인요청이면

        if(identified_flag == 0) // 고객 로그인 로직
        {
            login_pw_msg.clear();
            ID = read(clnt_sock,database);
            login_pw_msg.emplace_back(ID); 
            PW = read(clnt_sock,database);
            login_pw_msg.emplace_back(PW); 
            login_flag = login_instance.user_Login_logic(clnt_sock,login_pw_msg);
        }

        else // 직원 로그인 로직
        {
            login_pw_msg.clear();
            ID = read(clnt_sock,database);
            login_pw_msg.emplace_back(ID); 
            PW = read(clnt_sock,database);
            login_pw_msg.emplace_back(PW); 
            login_flag = login_instance.counselor_Login_logic(clnt_sock,login_pw_msg);
        }
    }

    else if(compare_msg == "회원가입요청") //회원가입 요청이면
    {
        if(identified_flag == 0) // 고객 로그인 로직
        {
            signup_pw_msg.clear();
            ID = read(clnt_sock,database);
            signup_pw_msg.emplace_back(ID); 
            PW = read(clnt_sock,database);
            signup_pw_msg.emplace_back(PW); 
            signup_instance.user_Signup_logic(clnt_sock,signup_pw_msg);
        }
        else if(identified_flag == 1)
        {
            signup_pw_msg.clear();
            ID = read(clnt_sock,database);
            signup_pw_msg.emplace_back(ID); 
            PW = read(clnt_sock,database);
            signup_pw_msg.emplace_back(PW); 
            signup_instance.counselor_Signup_logic(clnt_sock,signup_pw_msg);
        }
    }
    else
    {
        std::cout<<"로그인요청이나 회원가입요청이 아닌 값을 넘겨받음\n";
        exit(1);
    }

    if(login_flag == 0) //로그인 성공 반환값 0 /실패시 -1
        return 0;
    else
        return -1;
}

void * backgroundthread(void * arg)
{
    DB database;
    database.connect();
    std::string compare_msg;
    User_func user_func_instance(database);
    Counselor_func counselor_func_instance(database);
    int clnt_sock = *((int*) arg);
    int identified_flag=-1, lo_assign_flag= -1 ,login_flag = -1;
    char msg[3][BUF_SIZE];

    compare_msg=read(clnt_sock,database); // 고객인가 직원인가.
    std::cout<<compare_msg<<std::endl;
    if(compare_msg=="고객")
    {
        identified_flag = 0;
    }

    else if(compare_msg=="직원")
    {
        identified_flag = 1;
    }
    else
    {
        std::cout<<compare_msg<<std::endl;
        std::cout<<"고객이나 직원이 아닌 값을 넘겨받음\n";
        exit(1);
    }

    while(1)
    {
        if(login_flag = login_assign(clnt_sock,identified_flag,database) == 0) //로그인 성공해야만 브레이크
            break;
    }

    if(identified_flag == 0)
    {
        user_func_instance.choice_menu(clnt_sock);
    }
    else if(identified_flag == 1)
    {
        counselor_func_instance.choice_menu(clnt_sock);
    }    

    return NULL;    
}

size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);

    return size * nmemb;
}

void API(int clnt_sock,std::string want_word,std::string Do,DB& database){

    CURL* curl;
    CURLcode res;
    std::string readBuffer;

    std::string url = "https://stdict.korean.go.kr/api/search.do?certkey_no=6655&key=48341A1331E5300B0EF92C8B991629FB"
    "&type_search=search&req_type=json&num=10&q=";
    url += want_word;
    std::cout<<"curl_easy_init\n";
    curl = curl_easy_init();
    if (curl) {
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &readBuffer);
        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            fprintf(stderr, "curl_easy_perform() failed: %s\n", curl_easy_strerror(res));
        }
        curl_easy_cleanup(curl);
        // std::cout << readBuffer << std::endl;//json 모든값1
    }

    std::cout<<"json::parse\n";
    try{
        json jsonData = json::parse(readBuffer); //파싱드가자

        if (jsonData.contains("channel") && jsonData["channel"].contains("item")) {
            auto items = jsonData["channel"]["item"];
            json json_k_v;
            for (auto& item : items) {
                std::cout<<"auto& item : items\n";
                std::string word = item["word"];
                std::cout<<"std::string word = item[]\n";
                std::string definition = item["sense"]["definition"];
                std::cout<<"json_k_v[word] = definition;\n";
                json_k_v[word] = definition;
                std::cout<<"json_k_v.dump()\n";
                std::string jsonString = json_k_v.dump();
                if(Do == "미리보기")
                {
                    // preview_json.clear();
                    write(clnt_sock, jsonString.c_str(), strlen(jsonString.c_str()));
                    // preview_json.emplace_back(jsonString);
                }
                else if(Do == "추가")
                {
                    try
                    {
                        std::unique_ptr<sql::Statement> stmnt(database.conn->createStatement());
                        sql::ResultSet *res = stmnt->executeQuery("INSERT quiz values( '" + word + "', '" + definition + "')");
                    }
                    catch (sql::SQLException &e)
                    {
                        std::cerr << "Error user quiz 인서트 new task: " << e.what() << std::endl;
                        exit(1);
                    }
                }

                std::cout << "jsonString = "<<jsonString << std::endl;

                // std::cout << "--------------------------------------------------" << std::endl;
                // std::cout << "Word: " << word << std::endl;
                // std::cout << "Definition: " << definition << std::endl;
                // std::cout << "--------------------------" << std::endl;
                break;
            }
        }
        else {
            std::cerr << "Error: 'channel' or 'item' not found in JSON response" << std::endl;
        }

    }
    catch(nlohmann::json::parse_error& e){
        // preview_json.clear();
        std::cout<<"미리보기 결과없음\n";
        write(clnt_sock, "터짐", strlen("터짐"));
    }
}

void error_handling(const char* msg){
    fputs(msg,stderr);
    std::cerr<<'\n'<<stderr;
    exit(1);
}

void remove_clnt_serv(int clnt_sock,DB& database)
{
    pthread_mutex_lock(&mutx);
    try {
        std::unique_ptr < sql::Statement > stmnt(database.conn -> createStatement()); //객체 생성
        sql::ResultSet *res =stmnt->executeQuery("UPDATE user SET DISCRIPTER = -1 WHERE DISCRIPTER = "+std::__cxx11::to_string(clnt_sock));
    }
    catch(sql::SQLException& e){
        std::cerr << "Error user 로그아웃 0 업데이트 task: " << e.what() << std::endl;
    }
    try {
        std::unique_ptr < sql::Statement > stmnt(database.conn -> createStatement()); //객체 생성
        sql::ResultSet *res =stmnt->executeQuery("UPDATE counselor SET DISCRIPTER = -1 WHERE DISCRIPTER = "+std::__cxx11::to_string(clnt_sock));
    }
    catch(sql::SQLException& e){
        std::cerr << "Error counselor 로그아웃 0 업데이트 task: " << e.what() << std::endl;
    }
    for(int i = 0; i < clnt_cnt ; i++)
    {
        if(clnt_sock == clnt_socks[i])
        {
            while(i++<clnt_cnt-1)
                clnt_socks[i] = clnt_socks[i+1];
            break;
        }
    }
    clnt_cnt--;
    pthread_mutex_unlock(&mutx);
    close(clnt_sock);
}

std::string read(int clnt_sock,DB& data){
    std::string str_msg;
    char msg[BUF_SIZE];
    memset(msg,0,sizeof(msg));
    if(read(clnt_sock, msg, sizeof(msg))>0)
    {
        str_msg = msg;
        return str_msg;
    }
    else
    {
        remove_clnt_serv(clnt_sock,data);
        return "연결종료";
    }
}
