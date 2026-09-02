import "./Login.css";

function Login() {
  return (
    <>
      <div className="login-comp">
        <h2>Login</h2>
        <form>
          <ul>
            <li>
              <input className="inputField" placeholder="Email" type="Email" />
            </li>
            <li>
              <input
                className="inputField"
                placeholder="Password"
                type="password"
              />
            </li>
            <li className="row">
              <input type="checkbox"></input>
              <label>Remember me</label>
              <a href="#">Forgot Password?</a>
            </li>
            <li>
              <button>Login</button>
            </li>
          </ul>
        </form>

        <a href="#">Dont have an account? Sign up!</a>
      </div>
    </>
  );
}

export default Login;
