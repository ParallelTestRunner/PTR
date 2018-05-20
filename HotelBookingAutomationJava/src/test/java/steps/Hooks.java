package steps;

import pageObjectModel.WebManager;
import cucumber.api.java.After;
import cucumber.api.java.Before;

import java.util.HashMap;

public class Hooks {

    WebManager webManager;
    public WebManager getWebManager() {
        return webManager;
    }

    HashMap<String, Object> scenarioContext = new HashMap<String, Object>();
    public HashMap<String, Object> getScenarioContext()
    {
        return  scenarioContext;
    }

    @Before
    public void beforeTest() throws Exception {
        webManager = new WebManager();
        webManager.initialize();
    }

    @After
    public void afterTest() throws Exception {
        webManager.cleanup();
    }
}
